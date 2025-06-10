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
    public partial class S7Client
	{
		#region [Constants and TypeDefs]

		// Block type
		public const int Block_OB = 0x38;
		public const int Block_DB = 0x41;
		public const int Block_SDB = 0x42;
		public const int Block_FC = 0x43;
		public const int Block_SFC = 0x44;
		public const int Block_FB = 0x45;
		public const int Block_SFB = 0x46;

		// Sub Block Type 
		public const byte SubBlk_OB = 0x08;
		public const byte SubBlk_DB = 0x0A;
		public const byte SubBlk_SDB = 0x0B;
		public const byte SubBlk_FC = 0x0C;
		public const byte SubBlk_SFC = 0x0D;
		public const byte SubBlk_FB = 0x0E;
		public const byte SubBlk_SFB = 0x0F;

		// Block languages
		public const byte BlockLangAWL = 0x01;
		public const byte BlockLangKOP = 0x02;
		public const byte BlockLangFUP = 0x03;
		public const byte BlockLangSCL = 0x04;
		public const byte BlockLangDB = 0x05;
		public const byte BlockLangGRAPH = 0x06;

		// Max number of vars (multiread/write)
		public static readonly int MaxVars = 20;

		// Result transport size
		const byte TS_ResBit = 0x03;
		const byte TS_ResByte = 0x04;
		const byte TS_ResInt = 0x05;
		const byte TS_ResReal = 0x07;
		const byte TS_ResOctet = 0x09;

		const ushort Code7Ok = 0x0000;
		const ushort Code7AddressOutOfRange = 0x0005;
		const ushort Code7InvalidTransportSize = 0x0006;
		const ushort Code7WriteDataSizeMismatch = 0x0007;
		const ushort Code7ResItemNotAvailable = 0x000A;
		const ushort Code7ResItemNotAvailable1 = 0xD209;
		const ushort Code7InvalidValue = 0xDC01;
		const ushort Code7NeedPassword = 0xD241;
		const ushort Code7InvalidPassword = 0xD602;
		const ushort Code7NoPasswordToClear = 0xD604;
		const ushort Code7NoPasswordToSet = 0xD605;
		const ushort Code7FunNotAvailable = 0x8104;
		const ushort Code7DataOverPDU = 0x8500;

		// Client Connection Type
		public static readonly UInt16 CONNTYPE_PG = 0x01;  // Connect to the PLC as a PG
		public static readonly UInt16 CONNTYPE_OP = 0x02;  // Connect to the PLC as an OP
		public static readonly UInt16 CONNTYPE_BASIC = 0x03;  // Basic connection 

		public int _LastError = 0;

		#endregion

		#region [S7 Telegrams]

		// ISO Connection Request telegram (contains also ISO Header and COTP Header)
		byte[] ISO_CR = {
			// TPKT (RFC1006 Header)
			0x03, // RFC 1006 ID (3) 
			0x00, // Reserved, always 0
			0x00, // High part of packet lenght (entire frame, payload and TPDU included)
			0x16, // Low part of packet lenght (entire frame, payload and TPDU included)
			// COTP (ISO 8073 Header)
			0x11, // PDU Size Length
			0xE0, // CR - Connection Request ID
			0x00, // Dst Reference HI
			0x00, // Dst Reference LO
			0x00, // Src Reference HI
			0x01, // Src Reference LO
			0x00, // Class + Options Flags
			0xC0, // PDU Max Length ID
			0x01, // PDU Max Length HI
			0x0A, // PDU Max Length LO
			0xC1, // Src TSAP Identifier
			0x02, // Src TSAP Length (2 bytes)
			0x01, // Src TSAP HI (will be overwritten)
			0x00, // Src TSAP LO (will be overwritten)
			0xC2, // Dst TSAP Identifier
			0x02, // Dst TSAP Length (2 bytes)
			0x01, // Dst TSAP HI (will be overwritten)
			0x02  // Dst TSAP LO (will be overwritten)
		};

		// TPKT + ISO COTP Header (Connection Oriented Transport Protocol)
		byte[] TPKT_ISO = { // 7 bytes
			0x03,0x00,
			0x00,0x1f,      // Telegram Length (Data Size + 31 or 35)
			0x02,0xf0,0x80  // COTP (see above for info)
		};

		// S7 PDU Negotiation Telegram (contains also ISO Header and COTP Header)
		byte[] S7_PN = {
			0x03, 0x00, 0x00, 0x19,
			0x02, 0xf0, 0x80, // TPKT + COTP (see above for info)
			0x32, 0x01, 0x00, 0x00,
			0x04, 0x00, 0x00, 0x08,
			0x00, 0x00, 0xf0, 0x00,
			0x00, 0x01, 0x00, 0x01,
			0x00, 0x1e        // PDU Length Requested = HI-LO Here Default 480 bytes
		};

		// S7 Read/Write Request Header (contains also ISO Header and COTP Header)
		byte[] S7_RW = { // 31-35 bytes
			0x03,0x00,
			0x00,0x1f,       // Telegram Length (Data Size + 31 or 35)
			0x02,0xf0, 0x80, // COTP (see above for info)
			0x32,            // S7 Protocol ID 
			0x01,            // Job Type
			0x00,0x00,       // Redundancy identification
			0x05,0x00,       // PDU Reference
			0x00,0x0e,       // Parameters Length
			0x00,0x00,       // Data Length = Size(bytes) + 4      
			0x04,            // Function 4 Read Var, 5 Write Var  
			0x01,            // Items count
			0x12,            // Var spec.
			0x0a,            // Length of remaining bytes
			0x10,            // Syntax ID 
			(byte)S7Consts.S7WLByte,  // Transport Size idx=22                       
			0x00,0x00,       // Num Elements                          
			0x00,0x00,       // DB Number (if any, else 0)            
			0x84,            // Area Type                            
			0x00,0x00,0x00,  // Area Offset                     
			// WR area
			0x00,            // Reserved 
			0x04,            // Transport size
			0x00,0x00,       // Data Length * 8 (if not bit or timer or counter) 
		};
		private static int Size_RD = 31; // Header Size when Reading 
		private static int Size_WR = 35; // Header Size when Writing

		// S7 Variable MultiRead Header
		byte[] S7_MRD_HEADER = {
			0x03,0x00,
			0x00,0x1f,       // Telegram Length 
			0x02,0xf0, 0x80, // COTP (see above for info)
			0x32,            // S7 Protocol ID 
			0x01,            // Job Type
			0x00,0x00,       // Redundancy identification
			0x05,0x00,       // PDU Reference
			0x00,0x0e,       // Parameters Length
			0x00,0x00,       // Data Length = Size(bytes) + 4      
			0x04,            // Function 4 Read Var, 5 Write Var  
			0x01             // Items count (idx 18)
		};

		// S7 Variable MultiRead Item
		byte[] S7_MRD_ITEM = {
			0x12,            // Var spec.
			0x0a,            // Length of remaining bytes
			0x10,            // Syntax ID 
			(byte)S7Consts.S7WLByte,  // Transport Size idx=3                   
			0x00,0x00,       // Num Elements                          
			0x00,0x00,       // DB Number (if any, else 0)            
			0x84,            // Area Type                            
			0x00,0x00,0x00   // Area Offset                     
		};

		// S7 Variable MultiWrite Header
		byte[] S7_MWR_HEADER = {
			0x03,0x00,
			0x00,0x1f,       // Telegram Length 
			0x02,0xf0, 0x80, // COTP (see above for info)
			0x32,            // S7 Protocol ID 
			0x01,            // Job Type
			0x00,0x00,       // Redundancy identification
			0x05,0x00,       // PDU Reference
			0x00,0x0e,       // Parameters Length (idx 13)
			0x00,0x00,       // Data Length = Size(bytes) + 4 (idx 15)     
			0x05,            // Function 5 Write Var  
			0x01             // Items count (idx 18)
		};

		// S7 Variable MultiWrite Item (Param)
		byte[] S7_MWR_PARAM = {
			0x12,            // Var spec.
			0x0a,            // Length of remaining bytes
			0x10,            // Syntax ID 
			(byte)S7Consts.S7WLByte,  // Transport Size idx=3                      
			0x00,0x00,       // Num Elements                          
			0x00,0x00,       // DB Number (if any, else 0)            
			0x84,            // Area Type                            
			0x00,0x00,0x00,  // Area Offset                     
		};

		// SZL First telegram request   
		byte[] S7_SZL_FIRST = {
			0x03, 0x00, 0x00, 0x21,
			0x02, 0xf0, 0x80, 0x32,
			0x07, 0x00, 0x00,
			0x05, 0x00, // Sequence out
			0x00, 0x08, 0x00,
			0x08, 0x00, 0x01, 0x12,
			0x04, 0x11, 0x44, 0x01,
			0x00, 0xff, 0x09, 0x00,
			0x04,
			0x00, 0x00, // ID (29)
			0x00, 0x00  // Index (31)
		};

		// SZL Next telegram request 
		byte[] S7_SZL_NEXT = {
			0x03, 0x00, 0x00, 0x21,
			0x02, 0xf0, 0x80, 0x32,
			0x07, 0x00, 0x00, 0x06,
			0x00, 0x00, 0x0c, 0x00,
			0x04, 0x00, 0x01, 0x12,
			0x08, 0x12, 0x44, 0x01,
			0x01, // Sequence
			0x00, 0x00, 0x00, 0x00,
			0x0a, 0x00, 0x00, 0x00
		};

		// Get Date/Time request
		byte[] S7_GET_DT = {
			0x03, 0x00, 0x00, 0x1d,
			0x02, 0xf0, 0x80, 0x32,
			0x07, 0x00, 0x00, 0x38,
			0x00, 0x00, 0x08, 0x00,
			0x04, 0x00, 0x01, 0x12,
			0x04, 0x11, 0x47, 0x01,
			0x00, 0x0a, 0x00, 0x00,
			0x00
		};

		// Set Date/Time command
		byte[] S7_SET_DT = {
			0x03, 0x00, 0x00, 0x27,
			0x02, 0xf0, 0x80, 0x32,
			0x07, 0x00, 0x00, 0x89,
			0x03, 0x00, 0x08, 0x00,
			0x0e, 0x00, 0x01, 0x12,
			0x04, 0x11, 0x47, 0x02,
			0x00, 0xff, 0x09, 0x00,
			0x0a, 0x00,
			0x19, // Hi part of Year (idx=30)
			0x13, // Lo part of Year
			0x12, // Month
			0x06, // Day
			0x17, // Hour
			0x37, // Min
			0x13, // Sec
			0x00, 0x01 // ms + Day of week   
		};

		// S7 Set Session Password 
		byte[] S7_SET_PWD = {
			0x03, 0x00, 0x00, 0x25,
			0x02, 0xf0, 0x80, 0x32,
			0x07, 0x00, 0x00, 0x27,
			0x00, 0x00, 0x08, 0x00,
			0x0c, 0x00, 0x01, 0x12,
			0x04, 0x11, 0x45, 0x01,
			0x00, 0xff, 0x09, 0x00,
			0x08, 
			// 8 Char Encoded Password
			0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00
		};

		// S7 Clear Session Password 
		byte[] S7_CLR_PWD = {
			0x03, 0x00, 0x00, 0x1d,
			0x02, 0xf0, 0x80, 0x32,
			0x07, 0x00, 0x00, 0x29,
			0x00, 0x00, 0x08, 0x00,
			0x04, 0x00, 0x01, 0x12,
			0x04, 0x11, 0x45, 0x02,
			0x00, 0x0a, 0x00, 0x00,
			0x00
		};

		// S7 STOP request
		byte[] S7_STOP = {
			0x03, 0x00, 0x00, 0x21,
			0x02, 0xf0, 0x80, 0x32,
			0x01, 0x00, 0x00, 0x0e,
			0x00, 0x00, 0x10, 0x00,
			0x00, 0x29, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x09,
			0x50, 0x5f, 0x50, 0x52,
			0x4f, 0x47, 0x52, 0x41,
			0x4d
		};

		// S7 HOT Start request
		byte[] S7_HOT_START = {
			0x03, 0x00, 0x00, 0x25,
			0x02, 0xf0, 0x80, 0x32,
			0x01, 0x00, 0x00, 0x0c,
			0x00, 0x00, 0x14, 0x00,
			0x00, 0x28, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00,
			0xfd, 0x00, 0x00, 0x09,
			0x50, 0x5f, 0x50, 0x52,
			0x4f, 0x47, 0x52, 0x41,
			0x4d
		};

		// S7 COLD Start request
		byte[] S7_COLD_START = {
			0x03, 0x00, 0x00, 0x27,
			0x02, 0xf0, 0x80, 0x32,
			0x01, 0x00, 0x00, 0x0f,
			0x00, 0x00, 0x16, 0x00,
			0x00, 0x28, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00,
			0xfd, 0x00, 0x02, 0x43,
			0x20, 0x09, 0x50, 0x5f,
			0x50, 0x52, 0x4f, 0x47,
			0x52, 0x41, 0x4d
		};
		const byte pduStart = 0x28;   // CPU start
		const byte pduStop = 0x29;   // CPU stop
		const byte pduAlreadyStarted = 0x02;   // CPU already in run mode
		const byte pduAlreadyStopped = 0x07;   // CPU already in stop mode

		// S7 Get PLC Status 
		byte[] S7_GET_STAT = {
			0x03, 0x00, 0x00, 0x21,
			0x02, 0xf0, 0x80, 0x32,
			0x07, 0x00, 0x00, 0x2c,
			0x00, 0x00, 0x08, 0x00,
			0x08, 0x00, 0x01, 0x12,
			0x04, 0x11, 0x44, 0x01,
			0x00, 0xff, 0x09, 0x00,
			0x04, 0x04, 0x24, 0x00,
			0x00
		};

		// S7 Get Block Info Request Header (contains also ISO Header and COTP Header)
		byte[] S7_BI = {
			0x03, 0x00, 0x00, 0x25,
			0x02, 0xf0, 0x80, 0x32,
			0x07, 0x00, 0x00, 0x05,
			0x00, 0x00, 0x08, 0x00,
			0x0c, 0x00, 0x01, 0x12,
			0x04, 0x11, 0x43, 0x03,
			0x00, 0xff, 0x09, 0x00,
			0x08, 0x30,
			0x41, // Block Type
			0x30, 0x30, 0x30, 0x30, 0x30, // ASCII Block Number
			0x41
		};

		// S7 List Blocks Request Header
		byte[] S7_LIST_BLOCKS = {
			0x03, 0x00, 0x00, 0x1d,
			0x02, 0xf0, 0x80, 0x32,
			0x07, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x08, 0x00,
			0x04, 0x00, 0x01, 0x12,
			0x04, 0x11, 0x43, 0x01, // 0x43 0x01 = ListBlocks
			0x00, 0x0a, 0x00, 0x00,
			0x00
		};

		// S7 List Blocks Of Type Request Header
		byte[] S7_LIST_BLOCKS_OF_TYPE = {
			0x03, 0x00, 0x00, 0x1f,
			0x02, 0xf0, 0x80, 0x32,
			0x07, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x08, 0x00,
			0x06, 0x00, 0x01, 0x12,
			0x04, 0x11, 0x43, 0x02, // 0x43 0x02 = ListBlocksOfType
			0x00 // ... append ReqData
		};

		#endregion

		#region [Internals]

		// Defaults
		private static int ISOTCP = 102; // ISOTCP Port
		private static int MinPduSize = 16;
		private static int MinPduSizeToRequest = 240;
		private static int MaxPduSizeToRequest = 960;
		private static int DefaultTimeout = 2000;
		private static int IsoHSize = 7; // TPKT+COTP Header Size

		// Properties
		private int _PDULength = 0;
		private int _PduSizeRequested = 480;
		private int _PLCPort = ISOTCP;
		private int _RecvTimeout = DefaultTimeout;
		private int _SendTimeout = DefaultTimeout;
		private int _ConnTimeout = DefaultTimeout;

		// Privates
		private string IPAddress;
		private byte LocalTSAP_HI;
		private byte LocalTSAP_LO;
		private byte RemoteTSAP_HI;
		private byte RemoteTSAP_LO;
		private byte LastPDUType;
		private ushort ConnType = CONNTYPE_PG;
		private byte[] PDU = new byte[2048];
		private MsgSocket Socket = null;
		private int Time_ms = 0;
		private ushort cntword = 0;

		private void CreateSocket()
		{
			try
			{
				Socket = new MsgSocket();
				Socket.ConnectTimeout = _ConnTimeout;
				Socket.ReadTimeout = _RecvTimeout;
				Socket.WriteTimeout = _SendTimeout;
			}
			catch
			{
			}
		}

		private int TCPConnect()
		{
			if (_LastError == 0)
				try
				{
					_LastError = Socket.Connect(IPAddress, _PLCPort);
				}
				catch
				{
					_LastError = S7Consts.errTCPConnectionFailed;
				}
			return _LastError;
		}

		private void RecvPacket(byte[] Buffer, int Start, int Size)
		{
			if (Connected)
				_LastError = Socket.Receive(Buffer, Start, Size);
			else
				_LastError = S7Consts.errTCPNotConnected;
		}

		private void SendPacket(byte[] Buffer, int Len)
		{
			_LastError = Socket.Send(Buffer, Len);
		}

		private void SendPacket(byte[] Buffer)
		{
			if (Connected)
				SendPacket(Buffer, Buffer.Length);
			else
				_LastError = S7Consts.errTCPNotConnected;
		}

		private int RecvIsoPacket()
		{
			Boolean Done = false;
			int Size = 0;
			while ((_LastError == 0) && !Done)
			{
				// Get TPKT (4 bytes)
				RecvPacket(PDU, 0, 4);
				if (_LastError == 0)
				{
					Size = S7.GetWordAt(PDU, 2);
					// Check 0 bytes Data Packet (only TPKT+COTP = 7 bytes)
					if (Size == IsoHSize)
						RecvPacket(PDU, 4, 3); // Skip remaining 3 bytes and Done is still false
					else
					{
						if ((Size > _PduSizeRequested + IsoHSize) || (Size < MinPduSize))
							_LastError = S7Consts.errIsoInvalidPDU;
						else
							Done = true; // a valid Length !=7 && >16 && <247
					}
				}
			}
			if (_LastError == 0)
			{
				RecvPacket(PDU, 4, 3); // Skip remaining 3 COTP bytes
				LastPDUType = PDU[5];   // Stores PDU Type, we need it 
										// Receives the S7 Payload          
				RecvPacket(PDU, 7, Size - IsoHSize);
			}
			if (_LastError == 0)
				return Size;
			else
				return 0;
		}

		private int ISOConnect()
		{
			int Size;
			ISO_CR[16] = LocalTSAP_HI;
			ISO_CR[17] = LocalTSAP_LO;
			ISO_CR[20] = RemoteTSAP_HI;
			ISO_CR[21] = RemoteTSAP_LO;

			// Sends the connection request telegram      
			SendPacket(ISO_CR);
			if (_LastError == 0)
			{
				// Gets the reply (if any)
				Size = RecvIsoPacket();
				if (_LastError == 0)
				{
					if (Size == 22)
					{
						if (LastPDUType != (byte)0xD0) // 0xD0 = CC Connection confirm
							_LastError = S7Consts.errIsoConnect;
					}
					else
						_LastError = S7Consts.errIsoInvalidPDU;
				}
			}
			return _LastError;
		}

		private int NegotiatePduLength()
		{
			int Length;
			// Set PDU Size Requested
			S7.SetWordAt(S7_PN, 23, (ushort)_PduSizeRequested);
			// Sends the connection request telegram
			SendPacket(S7_PN);
			if (_LastError == 0)
			{
				Length = RecvIsoPacket();
				if (_LastError == 0)
				{
					// check S7 Error
					if ((Length == 27) && (PDU[17] == 0) && (PDU[18] == 0))  // 20 = size of Negotiate Answer
					{
						// Get PDU Size Negotiated
						_PDULength = S7.GetWordAt(PDU, 25);
						if (_PDULength <= 0)
							_LastError = S7Consts.errCliNegotiatingPDU;
					}
					else
						_LastError = S7Consts.errCliNegotiatingPDU;
				}
			}
			return _LastError;
		}

		private int CpuError(ushort Error)
		{
			switch (Error)
			{
				case 0: return 0;
				case Code7AddressOutOfRange: return S7Consts.errCliAddressOutOfRange;
				case Code7InvalidTransportSize: return S7Consts.errCliInvalidTransportSize;
				case Code7WriteDataSizeMismatch: return S7Consts.errCliWriteDataSizeMismatch;
				case Code7ResItemNotAvailable:
				case Code7ResItemNotAvailable1: return S7Consts.errCliItemNotAvailable;
				case Code7DataOverPDU: return S7Consts.errCliSizeOverPDU;
				case Code7InvalidValue: return S7Consts.errCliInvalidValue;
				case Code7FunNotAvailable: return S7Consts.errCliFunNotAvailable;
				case Code7NeedPassword: return S7Consts.errCliNeedPassword;
				case Code7InvalidPassword: return S7Consts.errCliInvalidPassword;
				case Code7NoPasswordToSet:
				case Code7NoPasswordToClear: return S7Consts.errCliNoPasswordToSetOrClear;
				default:
					return S7Consts.errCliFunctionRefused;
			};
		}

		private ushort GetNextWord()
		{
			return cntword++;
		}

		#endregion

		#region [Class Control]

		public S7Client()
		{
			CreateSocket();
		}

        // S7Client Destructor
        ~S7Client()
		{
			Disconnect();
		}

		public int Connect()
		{
			_LastError = 0;
			Time_ms = 0;
			int Elapsed = Environment.TickCount;
			if (!Connected)
			{
				TCPConnect(); // First stage : TCP Connection
				if (_LastError == 0)
				{
					ISOConnect(); // Second stage : ISOTCP (ISO 8073) Connection
					if (_LastError == 0)
					{
						_LastError = NegotiatePduLength(); // Third stage : S7 PDU negotiation
					}
				}
			}
			if (_LastError != 0)
				Disconnect();
			else
				Time_ms = Environment.TickCount - Elapsed;

			return _LastError;
		}

		public int ConnectTo(string Address, int Rack, int Slot)
		{
			UInt16 RemoteTSAP = (UInt16)((ConnType << 8) + (Rack * 0x20) + Slot);
			SetConnectionParams(Address, 0x0100, RemoteTSAP);
			return Connect();
		}

		public int SetConnectionParams(string Address, ushort LocalTSAP, ushort RemoteTSAP)
		{
			int LocTSAP = LocalTSAP & 0x0000FFFF;
			int RemTSAP = RemoteTSAP & 0x0000FFFF;
			IPAddress = Address;
			LocalTSAP_HI = (byte)(LocTSAP >> 8);
			LocalTSAP_LO = (byte)(LocTSAP & 0x00FF);
			RemoteTSAP_HI = (byte)(RemTSAP >> 8);
			RemoteTSAP_LO = (byte)(RemTSAP & 0x00FF);
			return 0;
		}

		public int SetConnectionType(ushort ConnectionType)
		{
			ConnType = ConnectionType;
			return 0;
		}

		public int Disconnect()
		{
			Socket.Close();
			return 0;
		}

		public int GetParam(Int32 ParamNumber, ref int Value)
		{
			int Result = 0;
			switch (ParamNumber)
			{
				case S7Consts.p_u16_RemotePort:
					{
						Value = PLCPort;
						break;
					}
				case S7Consts.p_i32_PingTimeout:
					{
						Value = ConnTimeout;
						break;
					}
				case S7Consts.p_i32_SendTimeout:
					{
						Value = SendTimeout;
						break;
					}
				case S7Consts.p_i32_RecvTimeout:
					{
						Value = RecvTimeout;
						break;
					}
				case S7Consts.p_i32_PDURequest:
					{
						Value = PduSizeRequested;
						break;
					}
				default:
					{
						Result = S7Consts.errCliInvalidParamNumber;
						break;
					}
			}
			return Result;
		}

		// Set Properties for compatibility with Snap7.net.cs
		public int SetParam(Int32 ParamNumber, ref int Value)
		{
			int Result = 0;
			switch (ParamNumber)
			{
				case S7Consts.p_u16_RemotePort:
					{
						PLCPort = Value;
						break;
					}
				case S7Consts.p_i32_PingTimeout:
					{
						ConnTimeout = Value;
						break;
					}
				case S7Consts.p_i32_SendTimeout:
					{
						SendTimeout = Value;
						break;
					}
				case S7Consts.p_i32_RecvTimeout:
					{
						RecvTimeout = Value;
						break;
					}
				case S7Consts.p_i32_PDURequest:
					{
						PduSizeRequested = Value;
						break;
					}
				default:
					{
						Result = S7Consts.errCliInvalidParamNumber;
						break;
					}
			}
			return Result;
		}

		public delegate void S7CliCompletion(IntPtr usrPtr, int opCode, int opResult);
		public int SetAsCallBack(S7CliCompletion Completion, IntPtr usrPtr)
		{
			return S7Consts.errCliFunctionNotImplemented;
		}

		#endregion

	}
}
