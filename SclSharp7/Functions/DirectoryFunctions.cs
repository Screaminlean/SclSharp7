namespace SclSharp7
{
    public partial class S7Client
    {
        #region [Directory functions]

        /// <summary>
        /// Lists all blocks in the PLC.
        /// </summary>
        /// <param name="List">Reference to S7BlocksList to receive the block list.</param>
        /// <returns>Error code.</returns>
        public int ListBlocks(ref S7BlocksList List)
        {
            _LastError = 0;
            Time_ms = 0;
            int Elapsed = Environment.TickCount;

            ushort Sequence = GetNextWord();

            Array.Copy(S7_LIST_BLOCKS, 0, PDU, 0, S7_LIST_BLOCKS.Length);
            PDU[0x0b] = (byte)(Sequence & 0xff);
            PDU[0x0c] = (byte)(Sequence >> 8);

            SendPacket(PDU, S7_LIST_BLOCKS.Length);

            if (_LastError != 0) return _LastError;
            int Length = RecvIsoPacket();
            if (Length <= 32)// the minimum expected
            {
                _LastError = S7Consts.errIsoInvalidPDU;
                return _LastError;
            }

            ushort Result = S7.GetWordAt(PDU, 27);
            if (Result != 0)
            {
                _LastError = CpuError(Result);
                return _LastError;
            }

            List = default(S7BlocksList);
            int BlocksSize = S7.GetWordAt(PDU, 31);

            if (Length <= 32 + BlocksSize)
            {
                _LastError = S7Consts.errIsoInvalidPDU;
                return _LastError;
            }

            int BlocksCount = BlocksSize >> 2;
            for (int blockNum = 0; blockNum < BlocksCount; blockNum++)
            {
                int Count = S7.GetWordAt(PDU, (blockNum << 2) + 35);

                switch (S7.GetByteAt(PDU, (blockNum << 2) + 34)) //BlockType
                {
                    case Block_OB:
                        List.OBCount = Count;
                        break;
                    case Block_DB:
                        List.DBCount = Count;
                        break;
                    case Block_SDB:
                        List.SDBCount = Count;
                        break;
                    case Block_FC:
                        List.FCCount = Count;
                        break;
                    case Block_SFC:
                        List.SFCCount = Count;
                        break;
                    case Block_FB:
                        List.FBCount = Count;
                        break;
                    case Block_SFB:
                        List.SFBCount = Count;
                        break;
                    default:
                        //Unknown block type. Ignore
                        break;
                }
            }

            Time_ms = Environment.TickCount - Elapsed;
            return _LastError; // 0
        }

        /// <summary>
        /// Asynchronously lists all blocks in the PLC.
        /// </summary>
        /// <param name="List">Reference to S7BlocksList to receive the block list.</param>
        /// <returns>Error code.</returns>
        public async Task<int> ListBlocksAsync(S7BlocksList List)
        {
            S7BlocksList localList = List;
            int result = await Task.Run(() => ListBlocks(ref localList));
            List = localList;
            return result;
        }

        /// <summary>
        /// Converts a Siemens encoded timestamp to a string.
        /// </summary>
        /// <param name="EncodedDate">The encoded date value.</param>
        /// <returns>Date string.</returns>
        private string SiemensTimestamp(long EncodedDate)
        {
            DateTime DT = new DateTime(1984, 1, 1).AddSeconds(EncodedDate * 86400);
#if WINDOWS_UWP || NETFX_CORE || CORE_CLR
			return DT.ToString(System.Globalization.DateTimeFormatInfo.CurrentInfo.ShortDatePattern);
#else
            return DT.ToShortDateString();
            //  return DT.ToString();
#endif
        }

        /// <summary>
        /// Gets block information from the PLC.
        /// </summary>
        /// <param name="BlockType">Block type.</param>
        /// <param name="BlockNum">Block number.</param>
        /// <param name="Info">Reference to S7BlockInfo to receive the block info.</param>
        /// <returns>Error code.</returns>
        public int GetAgBlockInfo(int BlockType, int BlockNum, ref S7BlockInfo Info)
        {
            _LastError = 0;
            Time_ms = 0;
            int Elapsed = Environment.TickCount;

            S7_BI[30] = (byte)BlockType;
            // Block Number
            S7_BI[31] = (byte)((BlockNum / 10000) + 0x30);
            BlockNum = BlockNum % 10000;
            S7_BI[32] = (byte)((BlockNum / 1000) + 0x30);
            BlockNum = BlockNum % 1000;
            S7_BI[33] = (byte)((BlockNum / 100) + 0x30);
            BlockNum = BlockNum % 100;
            S7_BI[34] = (byte)((BlockNum / 10) + 0x30);
            BlockNum = BlockNum % 10;
            S7_BI[35] = (byte)((BlockNum / 1) + 0x30);

            SendPacket(S7_BI);

            if (_LastError == 0)
            {
                int Length = RecvIsoPacket();
                if (Length > 32) // the minimum expected
                {
                    ushort Result = S7.GetWordAt(PDU, 27);
                    if (Result == 0)
                    {
                        Info.BlkFlags = PDU[42];
                        Info.BlkLang = PDU[43];
                        Info.BlkType = PDU[44];
                        Info.BlkNumber = S7.GetWordAt(PDU, 45);
                        Info.LoadSize = S7.GetDIntAt(PDU, 47);
                        Info.CodeDate = SiemensTimestamp(S7.GetWordAt(PDU, 59));
                        Info.IntfDate = SiemensTimestamp(S7.GetWordAt(PDU, 65));
                        Info.SBBLength = S7.GetWordAt(PDU, 67);
                        Info.LocalData = S7.GetWordAt(PDU, 71);
                        Info.MC7Size = S7.GetWordAt(PDU, 73);
                        Info.Author = S7.GetCharsAt(PDU, 75, 8).Trim(new char[] { (char)0 });
                        Info.Family = S7.GetCharsAt(PDU, 83, 8).Trim(new char[] { (char)0 });
                        Info.Header = S7.GetCharsAt(PDU, 91, 8).Trim(new char[] { (char)0 });
                        Info.Version = PDU[99];
                        Info.CheckSum = S7.GetWordAt(PDU, 101);
                    }
                    else
                        _LastError = CpuError(Result);
                }
                else
                    _LastError = S7Consts.errIsoInvalidPDU;
            }
            if (_LastError == 0)
                Time_ms = Environment.TickCount - Elapsed;

            return _LastError;

        }

        /// <summary>
        /// Asynchronously gets block information from the PLC.
        /// </summary>
        /// <param name="BlockType">Block type.</param>
        /// <param name="BlockNum">Block number.</param>
        /// <param name="Info">Reference to S7BlockInfo to receive the block info.</param>
        /// <returns>Error code.</returns>
        public async Task<int> GetAgBlockInfoAsync(int BlockType, int BlockNum, S7BlockInfo Info)
        {
            S7BlockInfo localInfo = Info;
            int result = await Task.Run(() => GetAgBlockInfo(BlockType, BlockNum, ref localInfo));
            Info = localInfo;
            return result;
        }

        /// <summary>
        /// Gets PG block information from the PLC. Not implemented.
        /// </summary>
        /// <param name="Info">Reference to S7BlockInfo to receive the block info.</param>
        /// <param name="Buffer">Buffer to use.</param>
        /// <param name="Size">Size of the buffer.</param>
        /// <returns>Error code.</returns>
        public int GetPgBlockInfo(ref S7BlockInfo Info, byte[] Buffer, int Size)
        {
            return S7Consts.errCliFunctionNotImplemented;
        }

        /// <summary>
        /// Asynchronously gets PG block information from the PLC. Not implemented.
        /// </summary>
        /// <param name="Info">Reference to S7BlockInfo to receive the block info.</param>
        /// <param name="Buffer">Buffer to use.</param>
        /// <param name="Size">Size of the buffer.</param>
        /// <returns>Error code.</returns>
        public async Task<int> GetPgBlockInfoAsync(S7BlockInfo Info, byte[] Buffer, int Size)
        {
            return await Task.Run(() => GetPgBlockInfo(ref Info, Buffer, Size));
        }

        /// <summary>
        /// Lists blocks of a specific type in the PLC.
        /// </summary>
        /// <param name="BlockType">Block type.</param>
        /// <param name="List">Array to receive block numbers.</param>
        /// <param name="ItemsCount">Reference to the number of items found.</param>
        /// <returns>Error code.</returns>
        public int ListBlocksOfType(int BlockType, ushort[] List, ref int ItemsCount)
        {
            var First = true;
            bool Done = false;
            byte In_Seq = 0;
            int Count = 0; //Block 1...n
            int PduLength;
            int Elapsed = Environment.TickCount;

            //Consequent packets have a different ReqData
            byte[] ReqData = new byte[] { 0xff, 0x09, 0x00, 0x02, 0x30, (byte)BlockType };
            byte[] ReqDataContinue = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0x00 };

            _LastError = 0;
            Time_ms = 0;

            do
            {
                PduLength = S7_LIST_BLOCKS_OF_TYPE.Length + ReqData.Length;
                ushort Sequence = GetNextWord();

                Array.Copy(S7_LIST_BLOCKS_OF_TYPE, 0, PDU, 0, S7_LIST_BLOCKS_OF_TYPE.Length);
                S7.SetWordAt(PDU, 0x02, (ushort)PduLength);
                PDU[0x0b] = (byte)(Sequence & 0xff);
                PDU[0x0c] = (byte)(Sequence >> 8);
                if (!First)
                {
                    S7.SetWordAt(PDU, 0x0d, 12); //ParLen
                    S7.SetWordAt(PDU, 0x0f, 4); //DataLen
                    PDU[0x14] = 8; //PLen
                    PDU[0x15] = 0x12; //Uk
                }
                PDU[0x17] = 0x02;
                PDU[0x18] = In_Seq;
                Array.Copy(ReqData, 0, PDU, 0x19, ReqData.Length);

                SendPacket(PDU, PduLength);
                if (_LastError != 0) return _LastError;

                PduLength = RecvIsoPacket();
                if (_LastError != 0) return _LastError;

                if (PduLength <= 32)// the minimum expected
                {
                    _LastError = S7Consts.errIsoInvalidPDU;
                    return _LastError;
                }

                ushort Result = S7.GetWordAt(PDU, 0x1b);
                if (Result != 0)
                {
                    _LastError = CpuError(Result);
                    return _LastError;
                }

                if (PDU[0x1d] != 0xFF)
                {
                    _LastError = S7Consts.errCliItemNotAvailable;
                    return _LastError;
                }

                Done = PDU[0x1a] == 0;
                In_Seq = PDU[0x18];

                int CThis = S7.GetWordAt(PDU, 0x1f) >> 2; //Amount of blocks in this message


                for (int c = 0; c < CThis; c++)
                {
                    if (Count >= ItemsCount) //RoomError
                    {
                        _LastError = S7Consts.errCliPartialDataRead;
                        return _LastError;
                    }
                    List[Count++] = S7.GetWordAt(PDU, 0x21 + 4 * c);
                    Done |= Count == 0x8000; //but why?
                }

                if (First)
                {
                    ReqData = ReqDataContinue;
                    First = false;
                }
            } while (_LastError == 0 && !Done);

            if (_LastError == 0)
                ItemsCount = Count;

            Time_ms = Environment.TickCount - Elapsed;
            return _LastError; // 0
        }

        /// <summary>
        /// Asynchronously lists blocks of a specific type in the PLC.
        /// </summary>
        /// <param name="BlockType">Block type.</param>
        /// <param name="List">Array to receive block numbers.</param>
        /// <param name="ItemsCount">Reference to the number of items found.</param>
        /// <returns>Error code.</returns>
        public async Task<int> ListBlocksOfTypeAsync(int BlockType, ushort[] List, int ItemsCount)
        {
            int localItemsCount = ItemsCount;
            int result = await Task.Run(() => ListBlocksOfType(BlockType, List, ref localItemsCount));
            ItemsCount = localItemsCount;
            return result;
        }

        #endregion
    }
}
