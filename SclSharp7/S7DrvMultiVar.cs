/*=============================================================================|
|  PROJECT Sharp7                                                        1.1.0 |
|==============================================================================|
|  Copyright (C) 2016 Davide Nardella                                          |
|  All rights reserved.                                                        |
|==============================================================================|
|  Sharp7 is free software: you can redistribute it and/or modify              |
|  it under the terms of the Lesser GNU General Public License as published by |
|  the Free Software Foundation, either version 3 of the License, or           |
|  (at your option) any later version.                                         |
|                                                                              |
|  It means that you can distribute your commercial software which includes    |
|  Sharp7 without the requirement to distribute the source code of your        |
|  application and without the requirement that your application be itself     |
|  distributed under LGPL.                                                     |
|                                                                              |
|  Sharp7 is distributed in the hope that it will be useful,                   |
|  but WITHOUT ANY WARRANTY; without even the implied warranty of              |
|  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the               |
|  Lesser GNU General Public License for more details.                         |
|                                                                              |
|  You should have received a copy of the GNU General Public License and a     |
|  copy of Lesser GNU General Public License along with Sharp7.                |
|  If not, see  http://www.gnu.org/licenses/                                   |
|==============================================================================|
History:
 * 1.0.0 2016/10/09 First Release
 * 1.0.1 2016/10/22 Added CoreCLR compatibility (CORE_CLR symbol must be 
					defined in Build options).
					Thanks to Dirk-Jan Wassink.
 * 1.0.2 2016/11/13 Fixed a bug in CLR compatibility
 * 1.0.3 2017/01/25 Fixed a bug in S7.GetIntAt(). Thanks to lupal1
					Added S7Timer Read/Write. Thanks to Lukas Palkovic 
 * 1.0.4 2018/06/12 Fixed the last bug in S7.GetIntAt(). Thanks to Jérémy HAURAY
					Get/Set LTime. Thanks to Jérémy HAURAY
					Get/Set 1500 WString. Thanks to Jérémy HAURAY
					Get/Set 1500 Array of WChar. Thanks to Jérémy HAURAY
 * 1.0.5 2018/11/21 Implemented ListBlocks and ListBlocksOfType (by Jos Koenis, TEB Engineering)
 * 1.0.6 2019/05/25 Implemented Force Jobs by Bart Swister
 * 1.0.7 2019/10/05 Bugfix in List in ListBlocksOfType. Thanks to Cosimo Ladiana 
 * ------------------------------------------------------------------------------
 * 1.1.0 2020/06/28 Implemented read/write Nck and Drive Data for Sinumerik 840D sl 
 *                  controls (by Chris Schöberlein) 
*/
using System.Runtime.InteropServices;
//------------------------------------------------------------------------------
// If you are compiling for UWP verify that WINDOWS_UWP or NETFX_CORE are 
// defined into Project Properties->Build->Conditional compilation symbols
//------------------------------------------------------------------------------
#if WINDOWS_UWP || NETFX_CORE
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else // <-- Including MONO
#endif

namespace SclSharp7
{
    #region [S7 Sinumerik]

    // S7 DriveES MultiRead and MultiWrite
    public class S7DrvMultiVar
	{
		#region [MultiRead/Write Helper]
		private S7Client FClient;
		private GCHandle[] Handles = new GCHandle[S7Client.MaxVars];
		private int Count = 0;
		private S7Client.S7DrvDataItem[] DrvItems = new S7Client.S7DrvDataItem[S7Client.MaxVars];
		public int[] Results = new int[S7Client.MaxVars];
		// Adapt WordLength
		private bool AdjustWordLength(int Area, ref int WordLen, ref int Amount, ref int Start)
		{
			// Calc Word size          
			int WordSize = S7.DataSizeByte(WordLen);
			if (WordSize == 0)
				return false;

			return true;
		}

		public S7DrvMultiVar(S7Client Client)
		{
			FClient = Client;
			for (int c = 0; c < S7Client.MaxVars; c++)
				Results[c] = (int)S7Consts.errCliItemNotAvailable;
		}

		~S7DrvMultiVar()
		{
			Clear();
		}

		// Add Drive Variables
		public bool DrvAdd<T>(S7DrvConsts.S7DrvTag Tag, ref T[] Buffer, int Offset)
		{
			return DrvAdd(Tag.DONumber, Tag.ParameterNumber, Tag.WordLen, Tag.Start, Tag.Elements, ref Buffer, Offset);
		}
		public bool DrvAdd<T>(S7DrvConsts.S7DrvTag Tag, ref T[] Buffer)
		{
			return DrvAdd(Tag.DONumber, Tag.ParameterNumber, Tag.WordLen, Tag.Start, Tag.Elements, ref Buffer);
		}
		public bool DrvAdd<T>(Int32 DONumber, Int32 ParameterNumber, Int32 WordLen, Int32 Start, ref T[] Buffer)
		{
			int Amount = 1;
			return DrvAdd(DONumber, ParameterNumber, WordLen, Start, Amount, ref Buffer, 0);
		}
		public bool DrvAdd<T>(Int32 DONumber, Int32 ParameterNumber, Int32 WordLen, Int32 Start, Int32 Amount, ref T[] Buffer)
		{
			return DrvAdd(DONumber, ParameterNumber, WordLen, Start, Amount, ref Buffer, 0);
		}
		public bool DrvAdd<T>(Int32 DONumber, Int32 ParameterNumber, Int32 WordLen, Int32 Start, ref T[] Buffer, int Offset)
		{
			int Amount = 1;
			return DrvAdd(DONumber, ParameterNumber, WordLen, Start, Amount, ref Buffer, Offset);
		}
		public bool DrvAdd<T>(Int32 DONumber, Int32 ParameterNumber, Int32 WordLen, Int32 Start, Int32 Amount, ref T[] Buffer, int Offset)
		{

			if (Count < S7Client.MaxVars)
			{
				//Syntax-ID for DriveES-Communication
				int DrvSynID = 162;
				if (AdjustWordLength(DrvSynID, ref WordLen, ref Amount, ref Start))
				{
					DrvItems[Count].DONumber = DONumber;
					DrvItems[Count].WordLen = WordLen;
					DrvItems[Count].Result = (int)S7Consts.errCliItemNotAvailable;
					DrvItems[Count].ParameterNumber = ParameterNumber;
					DrvItems[Count].Start = Start;
					DrvItems[Count].Amount = Amount;
					GCHandle handle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
#if WINDOWS_UWP || NETFX_CORE
                    if (IntPtr.Size == 4)
                        DrvItems[Count].pData = (IntPtr)(handle.AddrOfPinnedObject().ToInt32() + DataOffset * Marshal.SizeOf<T>());
                    else
                        DrvItems[Count].pData = (IntPtr)(handle.AddrOfPinnedObject().ToInt64() + DataOffset * Marshal.SizeOf<T>());
#else
					if (IntPtr.Size == 4)
						DrvItems[Count].pData = (IntPtr)(handle.AddrOfPinnedObject().ToInt32() + Offset * Marshal.SizeOf(typeof(T)));
					else
						DrvItems[Count].pData = (IntPtr)(handle.AddrOfPinnedObject().ToInt64() + Offset * Marshal.SizeOf(typeof(T)));
#endif
					Handles[Count] = handle;
					Count++;
					Offset = WordLen;
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}

		// Read Drive Parameter
		public int ReadDrv()
		{
			int FunctionResult;
			int GlobalResult = (int)S7Consts.errCliFunctionRefused;
			try
			{
				if (Count > 0)
				{
					FunctionResult = FClient.ReadMultiDrvVars(DrvItems, Count);
					if (FunctionResult == 0)
						for (int c = 0; c < S7Client.MaxVars; c++)
							Results[c] = DrvItems[c].Result;
					GlobalResult = FunctionResult;
				}
				else
					GlobalResult = S7Consts.errCliFunctionRefused;
			}
			finally
			{
				Clear(); // handles are no more needed and MUST be freed
			}
			return GlobalResult;
		}

		// Write Drive Parameter
		public int WriteDrv()
		{
			int FunctionResult;
			int GlobalResult = S7Consts.errCliFunctionRefused;
			try
			{
				if (Count > 0)
				{
					FunctionResult = FClient.WriteMultiDrvVars(DrvItems, Count);
					if (FunctionResult == 0)
						for (int c = 0; c < S7Client.MaxVars; c++)
							Results[c] = DrvItems[c].Result;
					GlobalResult = FunctionResult;
				}
				else
					GlobalResult = S7Consts.errCliFunctionRefused;
			}
			finally
			{
				Clear(); // handles are no more needed and MUST be freed
			}
			return GlobalResult;
		}

		public void Clear()
		{
			for (int c = 0; c < Count; c++)
			{
				if (Handles[c] != null)
					Handles[c].Free();
			}
			Count = 0;
		}
		#endregion
	}
	#endregion

}
