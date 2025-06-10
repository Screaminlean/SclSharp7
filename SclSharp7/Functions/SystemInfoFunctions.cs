namespace SclSharp7
{
    public partial class S7Client
    {
        #region [System Info functions]
        
        /// <summary>
        /// Gets the PLC order code information.
        /// </summary>
        /// <param name="Info">Reference to S7OrderCode to receive the order code information.</param>
        /// <returns>Error code.</returns>
        public int GetOrderCode(ref S7OrderCode Info)
        {
            S7SZL SZL = new S7SZL();
            int Size = 1024;
            SZL.Data = new byte[Size];
            int Elapsed = Environment.TickCount;
            _LastError = ReadSZL(0x0011, 0x000, ref SZL, ref Size);
            if (_LastError == 0)
            {
                Info.Code = S7.GetCharsAt(SZL.Data, 2, 20);
                Info.V1 = SZL.Data[Size - 3];
                Info.V2 = SZL.Data[Size - 2];
                Info.V3 = SZL.Data[Size - 1];
            }
            if (_LastError == 0)
                Time_ms = Environment.TickCount - Elapsed;
            return _LastError;
        }

        /// <summary>
        /// Asynchronously gets the PLC order code information.
        /// </summary>
        /// <param name="Info">Reference to S7OrderCode to receive the order code information.</param>
        /// <returns>Error code.</returns>
        public async Task<int> GetOrderCodeAsync(S7OrderCode Info)
        {
            S7OrderCode localInfo = Info;
            int result = await Task.Run(() => GetOrderCode(ref localInfo));
            Info = localInfo;
            return result;
        }

        /// <summary>
        /// Gets the PLC CPU information.
        /// </summary>
        /// <param name="Info">Reference to S7CpuInfo to receive the CPU information.</param>
        /// <returns>Error code.</returns>
        public int GetCpuInfo(ref S7CpuInfo Info)
        {
            S7SZL SZL = new S7SZL();
            int Size = 1024;
            SZL.Data = new byte[Size];
            int Elapsed = Environment.TickCount;
            _LastError = ReadSZL(0x001C, 0x000, ref SZL, ref Size);
            if (_LastError == 0)
            {
                Info.ModuleTypeName = S7.GetCharsAt(SZL.Data, 172, 32);
                Info.SerialNumber = S7.GetCharsAt(SZL.Data, 138, 24);
                Info.ASName = S7.GetCharsAt(SZL.Data, 2, 24);
                Info.Copyright = S7.GetCharsAt(SZL.Data, 104, 26);
                Info.ModuleName = S7.GetCharsAt(SZL.Data, 36, 24);
            }
            if (_LastError == 0)
                Time_ms = Environment.TickCount - Elapsed;
            return _LastError;
        }

        /// <summary>
        /// Asynchronously gets the PLC CPU information.
        /// </summary>
        /// <param name="Info">Reference to S7CpuInfo to receive the CPU information.</param>
        /// <returns>Error code.</returns>
        public async Task<int> GetCpuInfoAsync(S7CpuInfo Info)
        {
            S7CpuInfo localInfo = Info;
            int result = await Task.Run(() => GetCpuInfo(ref localInfo));
            Info = localInfo;
            return result;
        }

        /// <summary>
        /// Gets the PLC CP information.
        /// </summary>
        /// <param name="Info">Reference to S7CpInfo to receive the CP information.</param>
        /// <returns>Error code.</returns>
        public int GetCpInfo(ref S7CpInfo Info)
        {
            S7SZL SZL = new S7SZL();
            int Size = 1024;
            SZL.Data = new byte[Size];
            int Elapsed = Environment.TickCount;
            _LastError = ReadSZL(0x0131, 0x001, ref SZL, ref Size);
            if (_LastError == 0)
            {
                Info.MaxPduLength = S7.GetIntAt(PDU, 2);
                Info.MaxConnections = S7.GetIntAt(PDU, 4);
                Info.MaxMpiRate = S7.GetDIntAt(PDU, 6);
                Info.MaxBusRate = S7.GetDIntAt(PDU, 10);
            }
            if (_LastError == 0)
                Time_ms = Environment.TickCount - Elapsed;
            return _LastError;
        }

        /// <summary>
        /// Asynchronously gets the PLC CP information.
        /// </summary>
        /// <param name="Info">Reference to S7CpInfo to receive the CP information.</param>
        /// <returns>Error code.</returns>
        public async Task<int> GetCpInfoAsync(S7CpInfo Info)
        {
            S7CpInfo localInfo = Info;
            int result = await Task.Run(() => GetCpInfo(ref localInfo));
            Info = localInfo;
            return result;
        }

        /// <summary>
        /// Reads an SZL from the PLC.
        /// </summary>
        /// <param name="ID">SZL ID.</param>
        /// <param name="Index">SZL Index.</param>
        /// <param name="SZL">Reference to S7SZL to receive the SZL data.</param>
        /// <param name="Size">Reference to the size of the SZL data.</param>
        /// <returns>Error code.</returns>
        public int ReadSZL(int ID, int Index, ref S7SZL SZL, ref int Size)
        {
            int Length;
            int DataSZL;
            int Offset = 0;
            bool Done = false;
            bool First = true;
            byte Seq_in = 0x00;
            ushort Seq_out = 0x0000;

            _LastError = 0;
            Time_ms = 0;
            int Elapsed = Environment.TickCount;
            SZL.Header.LENTHDR = 0;

            do
            {
                if (First)
                {
                    S7.SetWordAt(S7_SZL_FIRST, 11, ++Seq_out);
                    S7.SetWordAt(S7_SZL_FIRST, 29, (ushort)ID);
                    S7.SetWordAt(S7_SZL_FIRST, 31, (ushort)Index);
                    SendPacket(S7_SZL_FIRST);
                }
                else
                {
                    S7.SetWordAt(S7_SZL_NEXT, 11, ++Seq_out);
                    PDU[24] = (byte)Seq_in;
                    SendPacket(S7_SZL_NEXT);
                }
                if (_LastError != 0)
                    return _LastError;

                Length = RecvIsoPacket();
                if (_LastError == 0)
                {
                    if (First)
                    {
                        if (Length > 32) // the minimum expected
                        {
                            if ((S7.GetWordAt(PDU, 27) == 0) && (PDU[29] == (byte)0xFF))
                            {
                                // Gets Amount of this slice
                                DataSZL = S7.GetWordAt(PDU, 31) - 8; // Skips extra params (ID, Index ...)
                                Done = PDU[26] == 0x00;
                                Seq_in = (byte)PDU[24]; // Slice sequence
                                SZL.Header.LENTHDR = S7.GetWordAt(PDU, 37);
                                SZL.Header.N_DR = S7.GetWordAt(PDU, 39);
                                Array.Copy(PDU, 41, SZL.Data, Offset, DataSZL);
                                //                                SZL.Copy(PDU, 41, Offset, DataSZL);
                                Offset += DataSZL;
                                SZL.Header.LENTHDR += SZL.Header.LENTHDR;
                            }
                            else
                                _LastError = S7Consts.errCliInvalidPlcAnswer;
                        }
                        else
                            _LastError = S7Consts.errIsoInvalidPDU;
                    }
                    else
                    {
                        if (Length > 32) // the minimum expected
                        {
                            if ((S7.GetWordAt(PDU, 27) == 0) && (PDU[29] == (byte)0xFF))
                            {
                                // Gets Amount of this slice
                                DataSZL = S7.GetWordAt(PDU, 31);
                                Done = PDU[26] == 0x00;
                                Seq_in = (byte)PDU[24]; // Slice sequence
                                Array.Copy(PDU, 37, SZL.Data, Offset, DataSZL);
                                Offset += DataSZL;
                                SZL.Header.LENTHDR += SZL.Header.LENTHDR;
                            }
                            else
                                _LastError = S7Consts.errCliInvalidPlcAnswer;
                        }
                        else
                            _LastError = S7Consts.errIsoInvalidPDU;
                    }
                }
                First = false;
            }
            while (!Done && (_LastError == 0));
            if (_LastError == 0)
            {
                Size = SZL.Header.LENTHDR;
                Time_ms = Environment.TickCount - Elapsed;
            }
            return _LastError;
        }

        /// <summary>
        /// Asynchronously reads an SZL from the PLC.
        /// </summary>
        /// <param name="ID">SZL ID.</param>
        /// <param name="Index">SZL Index.</param>
        /// <param name="SZL">Reference to S7SZL to receive the SZL data.</param>
        /// <param name="Size">Reference to the size of the SZL data.</param>
        /// <returns>Error code.</returns>
        public async Task<int> ReadSZLAsync(int ID, int Index, S7SZL SZL, int Size)
        {
            S7SZL localSZL = SZL;
            int localSize = Size;
            int result = await Task.Run(() => ReadSZL(ID, Index, ref localSZL, ref localSize));
            SZL = localSZL;
            Size = localSize;
            return result;
        }

        /// <summary>
        /// Reads the SZL list from the PLC. Not implemented.
        /// </summary>
        /// <param name="List">Reference to S7SZLList to receive the SZL list.</param>
        /// <param name="ItemsCount">Reference to the number of items found.</param>
        /// <returns>Error code.</returns>
        public int ReadSZLList(ref S7SZLList List, ref Int32 ItemsCount)
        {
            return S7Consts.errCliFunctionNotImplemented;
        }

        /// <summary>
        /// Asynchronously reads the SZL list from the PLC. Not implemented.
        /// </summary>
        /// <param name="List">Reference to S7SZLList to receive the SZL list.</param>
        /// <param name="ItemsCount">Reference to the number of items found.</param>
        /// <returns>Error code.</returns>
        public async Task<int> ReadSZLListAsync(S7SZLList List, int ItemsCount)
        {
            S7SZLList localList = List;
            int localItemsCount = ItemsCount;
            int result = await Task.Run(() => ReadSZLList(ref localList, ref localItemsCount));
            List = localList;
            ItemsCount = localItemsCount;
            return result;
        }

        #endregion
    }
}
