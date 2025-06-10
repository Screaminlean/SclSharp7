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

    #region [S7 Nck]
    // S7 Nck MultiRead and MultiWrite
    public class S7NckMultiVar
	{
		#region [MultiRead/Write Helper]
		private S7Client FClient;
		private GCHandle[] Handles = new GCHandle[S7Client.MaxVars];
		private int Count = 0;
		private S7Client.S7NckDataItem[] NckItems = new S7Client.S7NckDataItem[S7Client.MaxVars];
		public int[] Results = new int[S7Client.MaxVars];
		// Adapt WordLength
		private bool AdjustWordLength(int Area, ref int WordLen, ref int Amount, ref int Start)
		{
			// Calc Word size          
			int WordSize = S7Nck.NckDataSizeByte(WordLen);
			if (WordSize == 0)
				return false;

			if (WordLen == S7NckConsts.S7WLBit)
				Amount = 1;  // Only 1 bit can be transferred at time
			return true;
		}

		public S7NckMultiVar(S7Client Client)
		{
			FClient = Client;
			for (int c = 0; c < S7Client.MaxVars; c++)
				Results[c] = S7Consts.errCliItemNotAvailable;
		}

		~S7NckMultiVar()
		{
			Clear();
		}

		// Add Nck Variables
		public bool NckAdd<T>(S7NckConsts.S7NckTag Tag, ref T[] Buffer, int Offset)
		{
			return NckAdd(Tag.NckArea, Tag.NckUnit, Tag.NckModule, Tag.ParameterNumber, Tag.WordLen, Tag.Start, Tag.Elements, ref Buffer, Offset);
		}
		public bool NckAdd<T>(S7NckConsts.S7NckTag Tag, ref T[] Buffer)
		{
			return NckAdd(Tag.NckArea, Tag.NckUnit, Tag.NckModule, Tag.ParameterNumber, Tag.WordLen, Tag.Start, Tag.Elements, ref Buffer);
		}
		public bool NckAdd<T>(Int32 NckArea, Int32 NckUnit, Int32 NckModule, Int32 ParameterNumber, Int32 WordLen, Int32 Start, ref T[] Buffer)
		{
			int Amount = 1;
			return NckAdd(NckArea, NckUnit, NckModule, ParameterNumber, WordLen, Start, Amount, ref Buffer);
		}
		public bool NckAdd<T>(Int32 NckArea, Int32 NckUnit, Int32 NckModule, Int32 ParameterNumber, Int32 WordLen, Int32 Start, Int32 Amount, ref T[] Buffer)
		{
			return NckAdd(NckArea, NckUnit, NckModule, ParameterNumber, WordLen, Start, Amount, ref Buffer, 0);
		}
		public bool NckAdd<T>(Int32 NckArea, Int32 NckUnit, Int32 NckModule, Int32 ParameterNumber, Int32 WordLen, Int32 Start, Int32 Amount, ref T[] Buffer, int Offset)
		{
			if (Count < S7Client.MaxVars)
			{
				//Syntax-ID for Nck-Communication
				int NckSynID = 130;
				if (AdjustWordLength(NckSynID, ref WordLen, ref Amount, ref Start))
				{
					NckItems[Count].NckArea = NckArea;
					NckItems[Count].WordLen = WordLen;
					NckItems[Count].Result = (int)S7Consts.errCliItemNotAvailable;
					NckItems[Count].ParameterNumber = ParameterNumber;
					NckItems[Count].Start = Start;
					NckItems[Count].Amount = Amount;
					NckItems[Count].NckUnit = NckUnit;
					NckItems[Count].NckModule = NckModule;
					GCHandle handle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
#if WINDOWS_UWP || NETFX_CORE
                    if (IntPtr.Size == 4)
                        NckItems[Count].pData = (IntPtr)(handle.AddrOfPinnedObject().ToInt32() + Offset * Marshal.SizeOf<T>());
                    else
                        NckItems[Count].pData = (IntPtr)(handle.AddrOfPinnedObject().ToInt64() + Offset * Marshal.SizeOf<T>());
#else
					if (IntPtr.Size == 4)
						NckItems[Count].pData = (IntPtr)(handle.AddrOfPinnedObject().ToInt32() + Offset * Marshal.SizeOf(typeof(T)));
					else
						NckItems[Count].pData = (IntPtr)(handle.AddrOfPinnedObject().ToInt64() + Offset * Marshal.SizeOf(typeof(T)));
#endif
					Handles[Count] = handle;
					Count++;
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}

		//Read Nck Parameter
		public int ReadNck()
		{
			int FunctionResult;
			int GlobalResult = (int)S7Consts.errCliFunctionRefused;
			try
			{
				if (Count > 0)
				{
					FunctionResult = FClient.ReadMultiNckVars(NckItems, Count);
					if (FunctionResult == 0)
						for (int c = 0; c < S7Client.MaxVars; c++)
							Results[c] = NckItems[c].Result;
					GlobalResult = FunctionResult;
				}
				else
					GlobalResult = (int)S7Consts.errCliFunctionRefused;
			}
			finally
			{
				Clear(); // handles are no more needed and MUST be freed
			}
			return GlobalResult;
		}

		// Write Nck Parameter
		public int WriteNck()
		{
			int FunctionResult;
			int GlobalResult = (int)S7Consts.errCliFunctionRefused;
			try
			{
				if (Count > 0)
				{
					FunctionResult = FClient.WriteMultiNckVars(NckItems, Count);
					if (FunctionResult == 0)
						for (int c = 0; c < S7Client.MaxVars; c++)
							Results[c] = NckItems[c].Result;
					GlobalResult = FunctionResult;
				}
				else
					GlobalResult = (int)S7Consts.errCliFunctionRefused;
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

	#endregion [S7 Nck]

}
