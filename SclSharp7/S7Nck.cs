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
using System.Text;
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
    //$CS: Help Funktionen sind zu überarbeiten
    #region [S7 Nck  Help Functions]

    public static class S7Nck
	{

		private static Int64 bias = 621355968000000000; // "decimicros" between 0001-01-01 00:00:00 and 1970-01-01 00:00:00

		private static int BCDtoByte(byte B)
		{
			return ((B >> 4) * 10) + (B & 0x0F);
		}

		private static byte ByteToBCD(int Value)
		{
			return (byte)(((Value / 10) << 4) | (Value % 10));
		}

		private static byte[] CopyFrom(byte[] Buffer, int Pos, int Size)
		{
			byte[] Result = new byte[Size];
			Array.Copy(Buffer, Pos, Result, 0, Size);
			return Result;
		}

		private static byte[] OctRev(byte[] bytes, int Size)
		{
			byte[] reverse = new byte[Size];
			int j = 0;
			for (int i = Size - 1; i >= 0; i--)
			{
				reverse[j] = bytes[i];
				j++;
			}
			return reverse;

		}

		//S7 Nck Constants
		public static int NckDataSizeByte(int WordLength)
		{
			switch (WordLength)
			{
				case S7NckConsts.S7WLBit: return 1;  // S7 sends 1 byte per bit
				case S7NckConsts.S7WLByte: return 1;
				case S7NckConsts.S7WLChar: return 1;
				case S7NckConsts.S7WLWord: return 2;
				case S7NckConsts.S7WLDWord: return 4;
				case S7NckConsts.S7WLInt: return 2;
				case S7NckConsts.S7WLDInt: return 4;
				case S7NckConsts.S7WLDouble: return 8;
				case S7NckConsts.S7WLString: return 16;
				default: return 0;
			}
		}

		#region Get/Set the bit at Pos.Bit
		public static bool GetBitAt(byte[] Buffer, int Pos, int Bit)
		{
			byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
			if (Bit < 0) Bit = 0;
			if (Bit > 7) Bit = 7;
			return (Buffer[Pos] & Mask[Bit]) != 0;
		}
		public static void SetBitAt(ref byte[] Buffer, int Pos, int Bit, bool Value)
		{
			byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
			if (Bit < 0) Bit = 0;
			if (Bit > 7) Bit = 7;

			if (Value)
				Buffer[Pos] = (byte)(Buffer[Pos] | Mask[Bit]);
			else
				Buffer[Pos] = (byte)(Buffer[Pos] & ~Mask[Bit]);
		}
		#endregion

		#region Get/Set 8 bit signed value (S7 SInt) -128..127
		public static int GetSIntAt(byte[] Buffer, int Pos)
		{
			int Value = Buffer[Pos];
			if (Value < 128)
				return Value;
			else
				return (int)(Value - 256);
		}
		public static void SetSIntAt(byte[] Buffer, int Pos, int Value)
		{
			if (Value < -128) Value = -128;
			if (Value > 127) Value = 127;
			Buffer[Pos] = (byte)Value;
		}
		#endregion

		#region Get/Set 16 bit signed value (S7 int) -32768..32767
		public static short GetIntAt(byte[] Buffer, int Pos)
		{
			return (short)((Buffer[Pos + 1] << 8) | Buffer[Pos]);
		}
		public static void SetIntAt(byte[] Buffer, int Pos, Int16 Value)
		{
			Buffer[Pos + 1] = (byte)(Value >> 8);
			Buffer[Pos] = (byte)(Value & 0x00FF);
		}
		#endregion

		#region Get/Set 32 bit signed value (S7 DInt) -2147483648..2147483647
		public static int GetDIntAt(byte[] Buffer, int Pos)
		{
			int Result;
			Result = Buffer[Pos + 3]; Result <<= 8;
			Result += Buffer[Pos + 2]; Result <<= 8;
			Result += Buffer[Pos + 1]; Result <<= 8;
			Result += Buffer[Pos];
			return Result;
		}
		public static void SetDIntAt(byte[] Buffer, int Pos, int Value)
		{
			Buffer[Pos] = (byte)(Value & 0xFF);
			Buffer[Pos + 1] = (byte)((Value >> 8) & 0xFF);
			Buffer[Pos + 2] = (byte)((Value >> 16) & 0xFF);
			Buffer[Pos + 3] = (byte)((Value >> 24) & 0xFF);
		}
		#endregion

		#region Get/Set 64 bit signed value (S7 LInt) -9223372036854775808..9223372036854775807
		public static Int64 GetLIntAt(byte[] Buffer, int Pos)
		{
			Int64 Result;
			Result = Buffer[Pos + 7]; Result <<= 8;
			Result += Buffer[Pos + 6]; Result <<= 8;
			Result += Buffer[Pos + 5]; Result <<= 8;
			Result += Buffer[Pos + 4]; Result <<= 8;
			Result += Buffer[Pos + 3]; Result <<= 8;
			Result += Buffer[Pos + 2]; Result <<= 8;
			Result += Buffer[Pos + 1]; Result <<= 8;
			Result += Buffer[Pos];
			return Result;
		}
		public static void SetLIntAt(byte[] Buffer, int Pos, Int64 Value)
		{
			Buffer[Pos] = (byte)(Value & 0xFF);
			Buffer[Pos + 1] = (byte)((Value >> 8) & 0xFF);
			Buffer[Pos + 2] = (byte)((Value >> 16) & 0xFF);
			Buffer[Pos + 3] = (byte)((Value >> 24) & 0xFF);
			Buffer[Pos + 4] = (byte)((Value >> 32) & 0xFF);
			Buffer[Pos + 5] = (byte)((Value >> 40) & 0xFF);
			Buffer[Pos + 6] = (byte)((Value >> 48) & 0xFF);
			Buffer[Pos + 7] = (byte)((Value >> 56) & 0xFF);
		}
		#endregion

		#region Get/Set 8 bit unsigned value (S7 USInt) 0..255
		public static byte GetUSIntAt(byte[] Buffer, int Pos)
		{
			return Buffer[Pos];
		}
		public static void SetUSIntAt(byte[] Buffer, int Pos, byte Value)
		{
			Buffer[Pos] = Value;
		}
		#endregion

		#region Get/Set 16 bit unsigned value (S7 UInt) 0..65535
		public static UInt16 GetUIntAt(byte[] Buffer, int Pos)
		{
			return (UInt16)((Buffer[Pos + 1] << 8) | Buffer[Pos]);
		}
		public static void SetUIntAt(byte[] Buffer, int Pos, UInt16 Value)
		{
			Buffer[Pos + 1] = (byte)(Value >> 8);
			Buffer[Pos] = (byte)(Value & 0x00FF);
		}
		#endregion

		#region Get/Set 32 bit unsigned value (S7 UDInt) 0..4294967296
		public static UInt32 GetUDIntAt(byte[] Buffer, int Pos)
		{
			UInt32 Result;
			Result = Buffer[Pos + 3]; Result <<= 8;
			Result |= Buffer[Pos + 2]; Result <<= 8;
			Result |= Buffer[Pos + 1]; Result <<= 8;
			Result |= Buffer[Pos];
			return Result;
		}
		public static void SetUDIntAt(byte[] Buffer, int Pos, UInt32 Value)
		{
			Buffer[Pos] = (byte)(Value & 0xFF);
			Buffer[Pos + 1] = (byte)((Value >> 8) & 0xFF);
			Buffer[Pos + 2] = (byte)((Value >> 16) & 0xFF);
			Buffer[Pos + 3] = (byte)((Value >> 24) & 0xFF);
		}
		#endregion

		#region Get/Set 64 bit unsigned value (S7 ULint) 0..18446744073709551616
		public static UInt64 GetULIntAt(byte[] Buffer, int Pos)
		{
			UInt64 Result;
			Result = Buffer[Pos + 7]; Result <<= 8;
			Result |= Buffer[Pos + 6]; Result <<= 8;
			Result |= Buffer[Pos + 5]; Result <<= 8;
			Result |= Buffer[Pos + 4]; Result <<= 8;
			Result |= Buffer[Pos + 3]; Result <<= 8;
			Result |= Buffer[Pos + 2]; Result <<= 8;
			Result |= Buffer[Pos + 1]; Result <<= 8;
			Result |= Buffer[Pos ];
			return Result;
		}
		public static void SetULintAt(byte[] Buffer, int Pos, UInt64 Value)
		{
			Buffer[Pos + 7] = (byte)(Value & 0xFF);
			Buffer[Pos + 6] = (byte)((Value >> 8) & 0xFF);
			Buffer[Pos + 5] = (byte)((Value >> 16) & 0xFF);
			Buffer[Pos + 4] = (byte)((Value >> 24) & 0xFF);
			Buffer[Pos + 3] = (byte)((Value >> 32) & 0xFF);
			Buffer[Pos + 2] = (byte)((Value >> 40) & 0xFF);
			Buffer[Pos + 1] = (byte)((Value >> 48) & 0xFF);
			Buffer[Pos] = (byte)((Value >> 56) & 0xFF);
		}
		#endregion

		#region Get/Set 8 bit word (S7 Byte) 16#00..16#FF
		public static byte GetByteAt(byte[] Buffer, int Pos)
		{
			return Buffer[Pos];
		}
		public static void SetByteAt(byte[] Buffer, int Pos, byte Value)
		{
			Buffer[Pos] = Value;
		}
		#endregion

		#region Get/Set 16 bit word (S7 Word) 16#0000..16#FFFF
		public static UInt16 GetWordAt(byte[] Buffer, int Pos)
		{
			return GetUIntAt(Buffer, Pos);
		}
		public static void SetWordAt(byte[] Buffer, int Pos, UInt16 Value)
		{
			SetUIntAt(Buffer, Pos, Value);
		}
		#endregion

		#region Get/Set 32 bit word (S7 DWord) 16#00000000..16#FFFFFFFF
		public static UInt32 GetDWordAt(byte[] Buffer, int Pos)
		{
			return GetUDIntAt(Buffer, Pos);
		}
		public static void SetDWordAt(byte[] Buffer, int Pos, UInt32 Value)
		{
			SetUDIntAt(Buffer, Pos, Value);
		}
		#endregion

		#region Get/Set 64 bit word (S7 LWord) 16#0000000000000000..16#FFFFFFFFFFFFFFFF
		public static UInt64 GetLWordAt(byte[] Buffer, int Pos)
		{

			return GetULIntAt(Buffer, Pos);
		}
		public static void SetLWordAt(byte[] Buffer, int Pos, UInt64 Value)
		{
			SetULintAt(Buffer, Pos, Value);
		}
		#endregion

		#region Get/Set 64 bit floating point number (S7 LReal) (Range of Double)
		public static Double GetLRealAt(byte[] Buffer, int Pos)
		{
			UInt64 Value = GetULIntAt(Buffer, Pos);			
			byte[] bytes = BitConverter.GetBytes(Value);
			return BitConverter.ToDouble(bytes, 0);
		}
		public static void SetLRealAt(byte[] Buffer, int Pos, Double Value)
		{
			byte[] FloatArray = BitConverter.GetBytes(Value);
			Buffer[Pos + 7] = FloatArray[7];
			Buffer[Pos + 6] = FloatArray[6];
			Buffer[Pos + 5] = FloatArray[5];
			Buffer[Pos + 4] = FloatArray[4];
			Buffer[Pos + 3] = FloatArray[3];
			Buffer[Pos + 2] = FloatArray[2];
			Buffer[Pos + 1] = FloatArray[1];
			Buffer[Pos] = FloatArray[0];
		}
		#endregion

		#region Get/Set String (Nck Octet String)
		public static string GetStringAt(byte[] Buffer, int Pos)
		{
			int size = 16;
			return Encoding.UTF8.GetString(Buffer, Pos, size);
		}

		public static void SetStringAt(byte[] Buffer, int Pos, string Value)
		{
			int size = Value.Length;
			Encoding.UTF8.GetBytes(Value, 0, size, Buffer, Pos);
		}

		#endregion



		
	}
	#endregion

}
