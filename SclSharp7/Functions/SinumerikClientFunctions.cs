using System.Runtime.InteropServices;

namespace SclSharp7
{
    public partial class S7Client
    {
        #region Sinumerik Client Functions

        #region S7DriveES Client Functions
        // The following functions were only tested with Sinumerik 840D Solution Line (no Power Line support)
        // Connection to Sinumerik-Drive Main CU: use slot number 9
        // Connection to Sinumerik-Drive NX-Extensions: slot number usually starts with 13 (check via starter for individual configuration)
        #region [S7 DriveES Telegrams]
        // S7 DriveES Read/Write Request Header (contains also ISO Header and COTP Header)
        byte[] S7_DrvRW = { // 31-35 bytes
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
        0xa2,            // Syntax ID 
        0x00,            // Empty --> Parameter Type                       
        0x00,0x00,       // Empty --> Number of Rows                          
        0x00,0x00,       // Empty --> Number of DriveObject          
        0x00,0x00,       // Empty --> Parameter Number                           
        0x00,0x00,       // Empty --> Parameter Index                     
        // WR area
        0x00,            // Reserved 
        0x04,            // Transport size
        0x00,0x00,       // Data Length * 8 (if not bit or timer or counter) 
    };

        // S7 Drv Variable MultiRead Header
        byte[] S7Drv_MRD_HEADER = {
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
    };

        // S7 Drv Variable MultiRead Item
        byte[] S7Drv_MRD_ITEM =
            {
        0x12,            // Var spec.
        0x0a,            // Length of remaining bytes
        0xa2,            // Syntax ID 
        0x00,            // Empty --> Parameter Type                       
        0x00,0x00,       // Empty --> Number of Rows                          
        0x00,0x00,       // Empty --> Number of DriveObject          
        0x00,0x00,       // Empty --> Parameter Number                           
        0x00,0x00,       // Empty --> Parameter Index                    
    };

        // S7 Drv Variable MultiWrite Header
        byte[] S7Drv_MWR_HEADER = {
        0x03,0x00,
        0x00,0x1f,       // Telegram Length (Data Size + 31 or 35)
        0x02,0xf0, 0x80, // COTP (see above for info)
        0x32,            // S7 Protocol ID 
        0x01,            // Job Type
        0x00,0x00,       // Redundancy identification
        0x05,0x00,       // PDU Reference
        0x00,0x0e,       // Parameters Length
        0x00,0x00,       // Data Length = Size(bytes) + 4      
        0x05,            // Function 4 Read Var, 5 Write Var  
        0x01,            // Items count
    };

        // S7 Drv Variable MultiWrite Item
        byte[] S7Drv_MWR_PARAM =
            {
        0x12,            // Var spec.
        0x0a,            // Length of remaining bytes
        0xa2,            // Syntax ID 
        0x00,            // Empty --> Parameter Type                       
        0x00,0x00,       // Empty --> Number of Rows                          
        0x00,0x00,       // Empty --> Number of DriveObject          
        0x00,0x00,       // Empty --> Parameter Number                           
        0x00,0x00,       // Empty --> Parameter Index                    
    };
        #endregion [S7 DriveES Telegrams]


        /// <summary>
        /// Data I/O main function: Read Drive Area
        /// Function reads one drive parameter and defined amount of indizes of this parameter
        /// </summary>
        /// <param name="DONumber"></param>
        /// <param name="ParameterNumber"></param>
        /// <param name="Start"></param>
        /// <param name="Amount"></param>
        /// <param name="WordLen"></param>
        /// <param name="Buffer"></param>
        /// <returns></returns>
        public int ReadDrvArea(int DONumber, int ParameterNumber, int Start, int Amount, int WordLen, byte[] Buffer)
        {
            int BytesRead = 0;
            return ReadDrvArea(DONumber, ParameterNumber, Start, Amount, WordLen, Buffer, ref BytesRead);
        }
        public int ReadDrvArea(int DONumber, int ParameterNumber, int Start, int Amount, int WordLen, byte[] Buffer, ref int BytesRead)
        {
            // Variables
            int NumElements;
            int MaxElements;
            int TotElements;
            int SizeRequested;
            int Length;
            int Offset = 0;
            int WordSize = 1;

            _LastError = 0;
            Time_ms = 0;
            int Elapsed = Environment.TickCount;
            // Calc Word size          
            WordSize = S7Drv.DrvDataSizeByte(WordLen);
            if (WordSize == 0)
                return S7Consts.errCliInvalidWordLen;
            MaxElements = (_PDULength - 18) / WordSize; // 18 = Reply telegram header
            TotElements = Amount;

            while ((TotElements > 0) && (_LastError == 0))
            {
                NumElements = TotElements;
                if (NumElements > MaxElements)
                    NumElements = MaxElements;

                SizeRequested = NumElements * WordSize;

                //$7+: Setup the telegram - New Implementation for Drive Parameters
                Array.Copy(S7_DrvRW, 0, PDU, 0, Size_RD);
                //set DriveParameters
                S7.SetWordAt(PDU, 23, (ushort)NumElements);
                S7.SetWordAt(PDU, 25, (ushort)DONumber);
                S7.SetWordAt(PDU, 27, (ushort)ParameterNumber);
                S7.SetWordAt(PDU, 29, (ushort)Start);
                PDU[22] = (byte)WordLen;

                SendPacket(PDU, Size_RD);
                if (_LastError == 0)
                {
                    Length = RecvIsoPacket();
                    if (_LastError == 0)
                    {
                        if (Length < 25)
                            _LastError = S7Consts.errIsoInvalidDataSize;
                        else
                        {
                            if (PDU[21] != 0xFF)
                                _LastError = CpuError(PDU[21]);
                            else
                            {
                                Array.Copy(PDU, 25, Buffer, Offset, SizeRequested);
                                Offset += SizeRequested;
                            }
                        }
                    }
                }
                TotElements -= NumElements;
                Start += NumElements;




            }

            if (_LastError == 0)
            {
                BytesRead = Offset;
                Time_ms = Environment.TickCount - Elapsed;
            }
            else
                BytesRead = 0;
            return _LastError;
        }

        /// <summary>
        /// Data I/O main function: Read Multiple Drive Values
        /// </summary>
        /// <param name="Items"></param>
        /// <param name="ItemsCount"></param>
        /// <returns></returns>
        public int ReadMultiDrvVars(S7DrvDataItem[] Items, int ItemsCount)
        {
            int Offset;
            int Length;
            int ItemSize;
            byte[] S7DrvItem = new byte[12];
            byte[] S7DrvItemRead = new byte[1024];

            _LastError = 0;
            Time_ms = 0;
            int Elapsed = Environment.TickCount;

            // Checks items
            if (ItemsCount > MaxVars)
                return S7Consts.errCliTooManyItems;

            // Fills Header
            Array.Copy(S7Drv_MRD_HEADER, 0, PDU, 0, S7Drv_MRD_HEADER.Length);
            S7.SetWordAt(PDU, 13, (ushort)(ItemsCount * S7DrvItem.Length + 2));
            PDU[18] = (byte)ItemsCount;
            // Fills the Items
            Offset = 19;
            for (int c = 0; c < ItemsCount; c++)
            {
                Array.Copy(S7Drv_MRD_ITEM, S7DrvItem, S7DrvItem.Length);
                S7DrvItem[3] = (byte)Items[c].WordLen;
                S7.SetWordAt(S7DrvItem, 4, (ushort)Items[c].Amount);
                S7.SetWordAt(S7DrvItem, 6, (ushort)Items[c].DONumber);
                S7.SetWordAt(S7DrvItem, 8, (ushort)Items[c].ParameterNumber);
                S7.SetWordAt(S7DrvItem, 10, (ushort)Items[c].Start);


                Array.Copy(S7DrvItem, 0, PDU, Offset, S7DrvItem.Length);
                Offset += S7DrvItem.Length;
            }

            if (Offset > _PDULength)
                return S7Consts.errCliSizeOverPDU;

            S7.SetWordAt(PDU, 2, (ushort)Offset); // Whole size
            SendPacket(PDU, Offset);

            if (_LastError != 0)
                return _LastError;
            // Get Answer
            Length = RecvIsoPacket();
            if (_LastError != 0)
                return _LastError;
            // Check ISO Length
            if (Length < 22)
            {
                _LastError = S7Consts.errIsoInvalidPDU; // PDU too Small
                return _LastError;
            }
            // Check Global Operation Result
            _LastError = CpuError(S7.GetWordAt(PDU, 17));
            if (_LastError != 0)
                return _LastError;
            // Get true ItemsCount
            int ItemsRead = S7.GetByteAt(PDU, 20);
            if ((ItemsRead != ItemsCount) || (ItemsRead > MaxVars))
            {
                _LastError = S7Consts.errCliInvalidPlcAnswer;
                return _LastError;
            }
            // Get Data
            Offset = 21;
            for (int c = 0; c < ItemsCount; c++)
            {
                // Get the Item
                Array.Copy(PDU, Offset, S7DrvItemRead, 0, Length - Offset);
                if (S7DrvItemRead[0] == 0xff)
                {
                    ItemSize = (int)S7.GetWordAt(S7DrvItemRead, 2);
                    if ((S7DrvItemRead[1] != TS_ResOctet) && (S7DrvItemRead[1] != TS_ResReal) && (S7DrvItemRead[1] != TS_ResBit))
                        ItemSize = ItemSize >> 3;
                    Marshal.Copy(S7DrvItemRead, 4, Items[c].pData, ItemSize);
                    Items[c].Result = 0;
                    if (ItemSize % 2 != 0)
                        ItemSize++; // Odd size are rounded
                    Offset = Offset + 4 + ItemSize;
                }
                else
                {
                    Items[c].Result = CpuError(S7DrvItemRead[0]);
                    Offset += 4; // Skip the Item header                           
                }
            }
            Time_ms = Environment.TickCount - Elapsed;
            return _LastError;
        }

        /// <summary>
        /// Data I/O main function: Write Multiple Drive Values
        /// </summary>
        /// <param name="Items"></param>
        /// <param name="ItemsCount"></param>
        /// <returns></returns>
        public int WriteMultiDrvVars(S7DrvDataItem[] Items, int ItemsCount)
        {
            int Offset;
            int ParLength;
            int DataLength;
            int ItemDataSize = 4; //default
            byte[] S7DrvParItem = new byte[S7Drv_MWR_PARAM.Length];
            byte[] S7DrvDataItem = new byte[1024];

            _LastError = 0;
            Time_ms = 0;
            int Elapsed = Environment.TickCount;

            // Checks items
            if (ItemsCount > MaxVars)
                return S7Consts.errCliTooManyItems;
            // Fills Header
            Array.Copy(S7Drv_MWR_HEADER, 0, PDU, 0, S7Drv_MWR_HEADER.Length);
            ParLength = ItemsCount * S7Drv_MWR_PARAM.Length + 2;
            S7.SetWordAt(PDU, 13, (ushort)ParLength);
            PDU[18] = (byte)ItemsCount;
            // Fills Params
            Offset = S7Drv_MWR_HEADER.Length;
            for (int c = 0; c < ItemsCount; c++)
            {
                Array.Copy(S7Drv_MWR_PARAM, 0, S7DrvParItem, 0, S7Drv_MWR_PARAM.Length);
                S7DrvParItem[3] = (byte)Items[c].WordLen;
                S7.SetWordAt(S7DrvParItem, 4, (ushort)Items[c].Amount);
                S7.SetWordAt(S7DrvParItem, 6, (ushort)Items[c].DONumber);
                S7.SetWordAt(S7DrvParItem, 8, (ushort)Items[c].ParameterNumber);
                S7.SetWordAt(S7DrvParItem, 10, (ushort)Items[c].Start);

                Array.Copy(S7DrvParItem, 0, PDU, Offset, S7DrvParItem.Length);
                Offset += S7Drv_MWR_PARAM.Length;
            }
            // Fills Data
            DataLength = 0;
            for (int c = 0; c < ItemsCount; c++)
            {
                S7DrvDataItem[0] = 0x00;
                switch (Items[c].WordLen)
                {
                    case S7DrvConsts.S7WLReal:
                        S7DrvDataItem[1] = TS_ResReal;      // Real
                        ItemDataSize = 4;
                        break;
                    case S7DrvConsts.S7WLDWord:            // DWord
                        S7DrvDataItem[1] = TS_ResByte;
                        ItemDataSize = 4;
                        break;
                    case S7DrvConsts.S7WLDInt:            // DWord
                        S7DrvDataItem[1] = TS_ResByte;
                        ItemDataSize = 4;
                        break;
                    default:
                        S7DrvDataItem[1] = TS_ResByte;     // byte/word/int etc.
                        ItemDataSize = 2;
                        break;
                }
                ;


                if ((S7DrvDataItem[1] != TS_ResOctet) && (S7DrvDataItem[1] != TS_ResBit) && (S7DrvDataItem[1] != TS_ResReal))
                    S7.SetWordAt(S7DrvDataItem, 2, (ushort)(ItemDataSize * 8));
                else
                    S7.SetWordAt(S7DrvDataItem, 2, (ushort)ItemDataSize);

                Marshal.Copy(Items[c].pData, S7DrvDataItem, 4, ItemDataSize);

                if (ItemDataSize % 2 != 0)
                {
                    S7DrvDataItem[ItemDataSize + 4] = 0x00;
                    ItemDataSize++;
                }
                Array.Copy(S7DrvDataItem, 0, PDU, Offset, ItemDataSize + 4);
                Offset = Offset + ItemDataSize + 4;
                DataLength = DataLength + ItemDataSize + 4;
            }

            // Checks the size
            if (Offset > _PDULength)
                return S7Consts.errCliSizeOverPDU;

            S7.SetWordAt(PDU, 2, (ushort)Offset); // Whole size
            S7.SetWordAt(PDU, 15, (ushort)DataLength); // Whole size
            SendPacket(PDU, Offset);

            RecvIsoPacket();
            if (_LastError == 0)
            {
                // Check Global Operation Result
                _LastError = CpuError(S7.GetWordAt(PDU, 17));
                if (_LastError != 0)
                    return _LastError;
                // Get true ItemsCount
                int ItemsWritten = S7.GetByteAt(PDU, 20);
                if ((ItemsWritten != ItemsCount) || (ItemsWritten > MaxVars))
                {
                    _LastError = S7Consts.errCliInvalidPlcAnswer;
                    return _LastError;
                }

                for (int c = 0; c < ItemsCount; c++)
                {
                    if (PDU[c + 21] == 0xFF)
                        Items[c].Result = 0;
                    else
                        Items[c].Result = CpuError((ushort)PDU[c + 21]);
                }
                Time_ms = Environment.TickCount - Elapsed;
            }
            return _LastError;
        }

        // S7 Drive Connection
        public int DrvConnectTo(string Address)
        {
            UInt16 RemoteTSAP = (UInt16)((ConnType << 8) + (0 * 0x20) + 9);
            // testen
            SetConnectionParams(Address, 0x0100, RemoteTSAP);
            return Connect();
        }
        // S7 Drive Connection with Slot
        public int DrvConnectTo(string Address, int Slot)
        {
            UInt16 RemoteTSAP = (UInt16)((ConnType << 8) + (0 * 0x20) + Slot);
            // testen
            SetConnectionParams(Address, 0x0100, RemoteTSAP);
            return Connect();
        }
        // S7 Drive Connection with Rack & Slot
        public int DrvConnectTo(string Address, int Rack, int Slot)
        {
            UInt16 RemoteTSAP = (UInt16)((ConnType << 8) + (Rack * 0x20) + Slot);
            // testen
            SetConnectionParams(Address, 0x0100, RemoteTSAP);
            return Connect();
        }

        #endregion S7DriveES Functions

        #region S7Nck Client Functions
        // Connection to Sinumerik NC: use slot number 3
        #region [S7 NckTelegrams]
        // Size NCK-Protocoll not equal to S7-Any-Protocoll
        private static int Size_NckRD = 29; // Header Size when Reading 
        private static int Size_NckWR = 33; // Header Size when Writing

        // S7 NCK Read/Write Request Header (contains also ISO Header and COTP Header)
        byte[] S7_NckRW = { // 31-35 bytes
            0x03,0x00,
            0x00,0x1d,       // Telegram Length (Data Size + 29 or 33)
            0x02,0xf0, 0x80, // COTP (see above for info)
            0x32,            // S7 Protocol ID 
            0x01,            // Job Type
            0x00,0x00,       // Redundancy identification
            0x05,0x00,       // PDU Reference
            0x00,0x0c,       // Parameters Length
            0x00,0x00,       // Data Length = Size(bytes) + 4      
            0x04,            // Function 4 Read Var, 5 Write Var  
            0x01,            // Items count
            0x12,            // Var spec.
            0x08,            // Length of remaining bytes
            0x82,            // Syntax ID 
            0x00,            // Empty --> NCK Area and Unit                     
            0x00,0x00,       // Empty --> Parameter Number                          
            0x00,0x00,       // Empty --> Parameter Index          
            0x00,            // Empty --> NCK Module (See NCVar-Selector for help)                           
            0x00,            // Empty --> Number of Rows                     
            // WR area
            0x00,            // Reserved 
            0x04,            // Transport size
            0x00,0x00,       // Data Length * 8 (if not bit or timer or counter) 
        };

        // S7 Nck Variable MultiRead Header
        byte[] S7Nck_MRD_HEADER = {
            0x03,0x00,
            0x00,0x1d,       // Telegram Length (Data Size + 29 or 33)
            0x02,0xf0, 0x80, // COTP (see above for info)
            0x32,            // S7 Protocol ID 
            0x01,            // Job Type
            0x00,0x00,       // Redundancy identification
            0x05,0x00,       // PDU Reference
            0x00,0x0c,       // Parameters Length
            0x00,0x00,       // Data Length = Size(bytes) + 4      
            0x04,            // Function 4 Read Var, 5 Write Var  
            0x01,            // Items count
        };

        // S7 Nck Variable MultiRead Item
        byte[] S7Nck_MRD_ITEM = {
            0x12,            // Var spec.
            0x08,            // Length of remaining bytes
            0x82,            // Syntax ID 
            0x00,            // Empty --> NCK Area and Unit                     
            0x00,0x00,       // Empty --> Parameter Number                          
            0x00,0x00,       // Empty --> Parameter Index          
            0x00,            // Empty --> NCK Module (See NCVar-Selector for help)                           
            0x00,            // Empty --> Number of Rows                     
        };

        // S7 Nck Variable MultiWrite Header
        byte[] S7Nck_MWR_HEADER = {
            0x03,0x00,
            0x00,0x1d,       // Telegram Length (Data Size + 29 or 33)
            0x02,0xf0, 0x80, // COTP (see above for info)
            0x32,            // S7 Protocol ID 
            0x01,            // Job Type
            0x00,0x00,       // Redundancy identification
            0x05,0x00,       // PDU Reference
            0x00,0x0c,       // Parameters Length
            0x00,0x00,       // Data Length = Size(bytes) + 4      
            0x05,            // Function 4 Read Var, 5 Write Var  
            0x01,            // Items count
        };

        // S7 Nck Variable MultiWrite Item
        byte[] S7Nck_MWR_PARAM = {
            0x12,            // Var spec.
            0x08,            // Length of remaining bytes
            0x82,            // Syntax ID 
            0x00,            // Empty --> NCK Area and Unit                     
            0x00,0x00,       // Empty --> Parameter Number                          
            0x00,0x00,       // Empty --> Parameter Index          
            0x00,            // Empty --> NCK Module (See NCVar-Selector for help)                           
            0x00,            // Empty --> Number of Rows                     
        };
        #endregion [S7 NckTelegrams]

        /// <summary>
        /// Data I/O main function: Read Nck Area
        /// Function reads one Nck parameter and defined amount of indizes of this parameter
        /// </summary>
        /// <param name="NckArea"></param>
        /// <param name="NckModule"></param>
        /// <param name="ParameterNumber"></param>
        /// <param name="Start"></param>
        /// <param name="Amount"></param>
        /// <param name="WordLen"></param>
        /// <param name="Buffer"></param>
        /// <returns></returns>
        public int ReadNckArea(int NckArea, int NckUnit, int NckModule, int ParameterNumber, int Start, int Amount, int WordLen, byte[] Buffer)
        {
            int BytesRead = 0;
            return ReadNckArea(NckArea, NckUnit, NckModule, ParameterNumber, Start, Amount, WordLen, Buffer, ref BytesRead);
        }
        public int ReadNckArea(int NckArea, int NckUnit, int NckModule, int ParameterNumber, int Start, int Amount, int WordLen, byte[] Buffer, ref int BytesRead)
        {
            // Variables
            int NumElements;
            int MaxElements;
            int TotElements;
            int SizeRequested;
            int Length;
            int Offset = 0;
            int WordSize = 1;

            _LastError = 0;
            Time_ms = 0;
            int Elapsed = Environment.TickCount;
            // Calc Word size 
            //New Definition used: NCKDataSizeByte         
            WordSize = S7Nck.NckDataSizeByte(WordLen);
            if (WordSize == 0)
                return S7Consts.errCliInvalidWordLen;
            MaxElements = (_PDULength - 18) / WordSize; // 18 = Reply telegram header
            TotElements = Amount;

            while ((TotElements > 0) && (_LastError == 0))
            {
                NumElements = TotElements;
                if (NumElements > MaxElements)
                    NumElements = MaxElements;

                SizeRequested = NumElements * WordSize;
                //Setup the telegram - New Implementation for NCK Parameters
                Array.Copy(S7_NckRW, 0, PDU, 0, Size_NckRD);
                //set NckParameters        
                NckArea = NckArea << 4;
                PDU[22] = (byte)(NckArea + NckUnit);
                S7.SetWordAt(PDU, 23, (ushort)ParameterNumber);
                S7.SetWordAt(PDU, 25, (ushort)Start);
                PDU[27] = (byte)NckModule;
                PDU[28] = (byte)NumElements;

                SendPacket(PDU, Size_NckRD);
                if (_LastError == 0)
                {
                    Length = RecvIsoPacket();
                    if (_LastError == 0)
                    {
                        if (Length < 25)
                            _LastError = S7Consts.errIsoInvalidDataSize;
                        else
                        {
                            if (PDU[21] != 0xFF)
                                _LastError = CpuError(PDU[21]);
                            else
                            {
                                Array.Copy(PDU, 25, Buffer, Offset, SizeRequested);
                                Offset += SizeRequested;
                            }
                        }
                    }
                }
                TotElements -= NumElements;
                Start += NumElements;
            }
            if (_LastError == 0)
            {
                BytesRead = Offset;
                Time_ms = Environment.TickCount - Elapsed;
            }
            else
                BytesRead = 0;
            return _LastError;
        }

        /// <summary>
        /// Data I/O main function: Read Multiple Nck Values
        /// </summary>
        /// <param name="Items"></param>
        /// <param name="ItemsCount"></param>
        /// <returns></returns>
        public int ReadMultiNckVars(S7NckDataItem[] Items, int ItemsCount)
        {
            int Offset;
            int Length;
            int ItemSize;
            byte[] S7NckItem = new byte[10];
            byte[] S7NckItemRead = new byte[1024];

            _LastError = 0;
            Time_ms = 0;
            int Elapsed = Environment.TickCount;

            // Checks items
            if (ItemsCount > MaxVars)
                return S7Consts.errCliTooManyItems;

            // Fills Header
            Array.Copy(S7Nck_MRD_HEADER, 0, PDU, 0, S7Nck_MRD_HEADER.Length);
            S7.SetWordAt(PDU, 13, (ushort)(ItemsCount * S7NckItem.Length + 2));
            PDU[18] = (byte)ItemsCount;
            // Fills the Items
            Offset = 19;
            for (int c = 0; c < ItemsCount; c++)
            {
                Array.Copy(S7Nck_MRD_ITEM, S7NckItem, S7NckItem.Length);
                int NckArea = Items[c].NckArea << 4;
                S7NckItem[3] = (byte)(NckArea + Items[c].NckUnit);
                S7.SetWordAt(S7NckItem, 4, (ushort)Items[c].ParameterNumber);
                S7.SetWordAt(S7NckItem, 6, (ushort)Items[c].Start);
                S7.SetByteAt(S7NckItem, 8, (byte)Items[c].NckModule);
                S7.SetByteAt(S7NckItem, 9, (byte)Items[c].Amount);

                Array.Copy(S7NckItem, 0, PDU, Offset, S7NckItem.Length);
                Offset += S7NckItem.Length;
            }

            if (Offset > _PDULength)
                return S7Consts.errCliSizeOverPDU;

            S7.SetWordAt(PDU, 2, (ushort)Offset); // Whole size
            SendPacket(PDU, Offset);

            if (_LastError != 0)
                return _LastError;
            // Get Answer
            Length = RecvIsoPacket();
            if (_LastError != 0)
                return _LastError;
            // Check ISO Length
            if (Length < 22)
            {
                _LastError = S7Consts.errIsoInvalidPDU; // PDU too Small
                return _LastError;
            }
            // Check Global Operation Result
            _LastError = CpuError(S7.GetWordAt(PDU, 17));
            if (_LastError != 0)
                return _LastError;
            // Get true ItemsCount
            int ItemsRead = S7.GetByteAt(PDU, 20);
            if ((ItemsRead != ItemsCount) || (ItemsRead > MaxVars))
            {
                _LastError = S7Consts.errCliInvalidPlcAnswer;
                return _LastError;
            }
            // Get Data
            Offset = 21;
            for (int c = 0; c < ItemsCount; c++)
            {
                // Get the Item
                Array.Copy(PDU, Offset, S7NckItemRead, 0, Length - Offset);
                if (S7NckItemRead[0] == 0xff)
                {
                    ItemSize = (int)S7.GetWordAt(S7NckItemRead, 2);
                    if ((S7NckItemRead[1] != TS_ResOctet) && (S7NckItemRead[1] != TS_ResReal) && (S7NckItemRead[1] != TS_ResBit))
                        ItemSize = ItemSize >> 3;
                    Marshal.Copy(S7NckItemRead, 4, Items[c].pData, ItemSize);
                    Items[c].Result = 0;
                    if (ItemSize % 2 != 0)
                        ItemSize++; // Odd size are rounded
                    Offset = Offset + 4 + ItemSize;
                }
                else
                {
                    Items[c].Result = CpuError(S7NckItemRead[0]);
                    Offset += 4; // Skip the Item header                           
                }
            }
            Time_ms = Environment.TickCount - Elapsed;
            return _LastError;
        }

        /// <summary>
        /// $7+ new Data I/O main function: Write Multiple Nck Values (under construction)
        /// </summary>
        /// <param name="Items"></param>
        /// <param name="ItemsCount"></param>
        /// <returns></returns>
        public int WriteMultiNckVars(S7NckDataItem[] Items, int ItemsCount)
        {
            int Offset;
            int ParLength;
            int DataLength;
            int ItemDataSize;
            byte[] S7NckParItem = new byte[S7Nck_MWR_PARAM.Length];
            byte[] S7NckDataItem = new byte[1024];

            _LastError = 0;
            Time_ms = 0;
            int Elapsed = Environment.TickCount;

            // Checks items
            if (ItemsCount > MaxVars)
                return S7Consts.errCliTooManyItems;
            // Fills Header
            Array.Copy(S7Nck_MWR_HEADER, 0, PDU, 0, S7Nck_MWR_HEADER.Length);
            ParLength = ItemsCount * S7Nck_MWR_PARAM.Length + 2;
            S7.SetWordAt(PDU, 13, (ushort)ParLength);
            PDU[18] = (byte)ItemsCount;
            // Fills Params
            Offset = S7Nck_MWR_HEADER.Length;
            for (int c = 0; c < ItemsCount; c++)
            {
                // Set Parameters
                Array.Copy(S7Nck_MWR_PARAM, 0, S7NckParItem, 0, S7Nck_MWR_PARAM.Length);
                int NckArea = Items[c].NckArea << 4;
                S7NckParItem[3] = (byte)(NckArea + Items[c].NckUnit);
                S7.SetWordAt(S7NckParItem, 4, (ushort)Items[c].ParameterNumber);
                S7.SetWordAt(S7NckParItem, 6, (ushort)Items[c].Start);
                S7.SetByteAt(S7NckParItem, 8, (byte)Items[c].NckModule);
                S7.SetByteAt(S7NckParItem, 9, (byte)Items[c].Amount);
                Array.Copy(S7NckParItem, 0, PDU, Offset, S7NckParItem.Length);
                Offset += S7Nck_MWR_PARAM.Length;
            }
            // Fills Data
            DataLength = 0;
            for (int c = 0; c < ItemsCount; c++)
            {
                S7NckDataItem[0] = 0x00;
                // All Nck-Parameters are written as octet-string
                S7NckDataItem[1] = TS_ResOctet;
                if (Items[c].WordLen == S7NckConsts.S7WLBit || Items[c].WordLen == S7Consts.S7WLByte)
                    ItemDataSize = 1;
                else if (Items[c].WordLen == S7NckConsts.S7WLDouble)
                    ItemDataSize = 8;
                else if (Items[c].WordLen == S7NckConsts.S7WLString)
                    ItemDataSize = 16;
                else
                    ItemDataSize = 4;


                if ((S7NckDataItem[1] != TS_ResOctet) && (S7NckDataItem[1] != TS_ResBit) && (S7NckDataItem[1] != TS_ResReal))
                    S7.SetWordAt(S7NckDataItem, 2, (ushort)(ItemDataSize * 8));
                else
                    S7.SetWordAt(S7NckDataItem, 2, (ushort)ItemDataSize);

                Marshal.Copy(Items[c].pData, S7NckDataItem, 4, ItemDataSize);

                if (ItemDataSize % 2 != 0)
                {
                    S7NckDataItem[ItemDataSize + 4] = 0x00;
                    ItemDataSize++;
                }
                Array.Copy(S7NckDataItem, 0, PDU, Offset, ItemDataSize + 4);
                Offset = Offset + ItemDataSize + 4;
                DataLength = DataLength + ItemDataSize + 4;
            }




            // Checks the size
            if (Offset > _PDULength)
                return S7Consts.errCliSizeOverPDU;

            S7.SetWordAt(PDU, 2, (ushort)Offset); // Whole size
            S7.SetWordAt(PDU, 15, (ushort)DataLength); // Whole size
            SendPacket(PDU, Offset);

            RecvIsoPacket();
            if (_LastError == 0)
            {
                // Check Global Operation Result
                _LastError = CpuError(S7.GetWordAt(PDU, 17));
                if (_LastError != 0)
                    return _LastError;
                // Get true ItemsCount
                int ItemsWritten = S7.GetByteAt(PDU, 20);
                if ((ItemsWritten != ItemsCount) || (ItemsWritten > MaxVars))
                {
                    _LastError = S7Consts.errCliInvalidPlcAnswer;
                    return _LastError;
                }

                for (int c = 0; c < ItemsCount; c++)
                {
                    if (PDU[c + 21] == 0xFF)
                        Items[c].Result = 0;
                    else
                        Items[c].Result = CpuError((ushort)PDU[c + 21]);
                }
                Time_ms = Environment.TickCount - Elapsed;
            }
            return _LastError;
        }

        // S7 Nck Connection
        public int NckConnectTo(string Address)
        {
            UInt16 RemoteTSAP = (UInt16)((ConnType << 8) + (0 * 0x20) + 3);
            // testen
            SetConnectionParams(Address, 0x0100, RemoteTSAP);
            return Connect();
        }
        // S7 Nck Connection with Rack
        public int NckConnectTo(string Address, int Rack)
        {
            UInt16 RemoteTSAP = (UInt16)((ConnType << 8) + (Rack * 0x20) + 3);
            // testen
            SetConnectionParams(Address, 0x0100, RemoteTSAP);
            return Connect();
        }

        #endregion S7Nck Client Functions

        #endregion Sinumerik Client Functions
    }
}
