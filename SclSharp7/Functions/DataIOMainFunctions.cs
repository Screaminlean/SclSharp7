using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SclSharp7
{
    public partial class S7Client
    {
        #region [Data I/O main functions]

        /// <summary>
        /// Reads an area of the PLC.
        /// </summary>
        /// <param name="Area">Area code.</param>
        /// <param name="DBNumber">DB number.</param>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Amount to read.</param>
        /// <param name="WordLen">Word length.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <returns>Error code.</returns>
        public int ReadArea(int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer)
        {
            int BytesRead = 0;
            return ReadArea(Area, DBNumber, Start, Amount, WordLen, Buffer, ref BytesRead);
        }

        /// <summary>
        /// Asynchronously reads an area of the PLC.
        /// </summary>
        /// <param name="Area">Area code.</param>
        /// <param name="DBNumber">DB number.</param>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Amount to read.</param>
        /// <param name="WordLen">Word length.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <returns>Error code.</returns>
        public async Task<int> ReadAreaAsync(int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer)
        {
            return await Task.Run(() => ReadArea(Area, DBNumber, Start, Amount, WordLen, Buffer));
        }

        /// <summary>
        /// Reads an area of the PLC with bytes read output.
        /// </summary>
        /// <param name="Area">Area code.</param>
        /// <param name="DBNumber">DB number.</param>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Amount to read.</param>
        /// <param name="WordLen">Word length.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <param name="BytesRead">Reference to bytes read.</param>
        /// <returns>Error code.</returns>
        public int ReadArea(int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer, ref int BytesRead)
        {
            int Address;
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
            // Some adjustment
            if (Area == S7Consts.S7AreaCT)
                WordLen = S7Consts.S7WLCounter;
            if (Area == S7Consts.S7AreaTM)
                WordLen = S7Consts.S7WLTimer;

            // Calc Word size          
            WordSize = S7.DataSizeByte(WordLen);
            if (WordSize == 0)
                return S7Consts.errCliInvalidWordLen;

            if (WordLen == S7Consts.S7WLBit)
                Amount = 1;  // Only 1 bit can be transferred at time
            else
            {
                if ((WordLen != S7Consts.S7WLCounter) && (WordLen != S7Consts.S7WLTimer))
                {
                    Amount = Amount * WordSize;
                    WordSize = 1;
                    WordLen = S7Consts.S7WLByte;
                }
            }

            MaxElements = (_PDULength - 18) / WordSize; // 18 = Reply telegram header
            TotElements = Amount;

            while ((TotElements > 0) && (_LastError == 0))
            {
                NumElements = TotElements;
                if (NumElements > MaxElements)
                    NumElements = MaxElements;

                SizeRequested = NumElements * WordSize;

                // Setup the telegram
                Array.Copy(S7_RW, 0, PDU, 0, Size_RD);
                // Set DB Number
                PDU[27] = (byte)Area;
                // Set Area
                if (Area == S7Consts.S7AreaDB)
                    S7.SetWordAt(PDU, 25, (ushort)DBNumber);

                // Adjusts Start and word length
                if ((WordLen == S7Consts.S7WLBit) || (WordLen == S7Consts.S7WLCounter) || (WordLen == S7Consts.S7WLTimer))
                {
                    Address = Start;
                    PDU[22] = (byte)WordLen;
                }
                else
                    Address = Start << 3;

                // Num elements
                S7.SetWordAt(PDU, 23, (ushort)NumElements);

                // Address into the PLC (only 3 bytes)           
                PDU[30] = (byte)(Address & 0x0FF);
                Address = Address >> 8;
                PDU[29] = (byte)(Address & 0x0FF);
                Address = Address >> 8;
                PDU[28] = (byte)(Address & 0x0FF);

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
                Start += NumElements * WordSize;
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
        /// Asynchronously reads an area of the PLC with bytes read output.
        /// </summary>
        /// <param name="Area">Area code.</param>
        /// <param name="DBNumber">DB number.</param>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Amount to read.</param>
        /// <param name="WordLen">Word length.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <param name="BytesRead">Reference to bytes read.</param>
        /// <returns>Error code.</returns>
        public async Task<int> ReadAreaAsync(int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer, int BytesRead)
        {
            int localBytesRead = BytesRead;
            int result = await Task.Run(() => ReadArea(Area, DBNumber, Start, Amount, WordLen, Buffer, ref localBytesRead));
            BytesRead = localBytesRead;
            return result;
        }

        /// <summary>
        /// Writes an area of the PLC.
        /// </summary>
        /// <param name="Area">Area code.</param>
        /// <param name="DBNumber">DB number.</param>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Amount to write.</param>
        /// <param name="WordLen">Word length.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <returns>Error code.</returns>
        public int WriteArea(int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer)
        {
            int BytesWritten = 0;
            return WriteArea(Area, DBNumber, Start, Amount, WordLen, Buffer, ref BytesWritten);
        }

        /// <summary>
        /// Asynchronously writes an area of the PLC.
        /// </summary>
        /// <param name="Area">Area code.</param>
        /// <param name="DBNumber">DB number.</param>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Amount to write.</param>
        /// <param name="WordLen">Word length.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <returns>Error code.</returns>
        public async Task<int> WriteAreaAsync(int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer)
        {
            return await Task.Run(() => WriteArea(Area, DBNumber, Start, Amount, WordLen, Buffer));
        }

        /// <summary>
        /// Writes an area of the PLC with bytes written output.
        /// </summary>
        /// <param name="Area">Area code.</param>
        /// <param name="DBNumber">DB number.</param>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Amount to write.</param>
        /// <param name="WordLen">Word length.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <param name="BytesWritten">Reference to bytes written.</param>
        /// <returns>Error code.</returns>
        public int WriteArea(int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer, ref int BytesWritten)
        {
            int Address;
            int NumElements;
            int MaxElements;
            int TotElements;
            int DataSize;
            int IsoSize;
            int Length;
            int Offset = 0;
            int WordSize = 1;

            _LastError = 0;
            Time_ms = 0;
            int Elapsed = Environment.TickCount;
            // Some adjustment
            if (Area == S7Consts.S7AreaCT)
                WordLen = S7Consts.S7WLCounter;
            if (Area == S7Consts.S7AreaTM)
                WordLen = S7Consts.S7WLTimer;

            // Calc Word size          
            WordSize = S7.DataSizeByte(WordLen);
            if (WordSize == 0)
                return S7Consts.errCliInvalidWordLen;

            if (WordLen == S7Consts.S7WLBit) // Only 1 bit can be transferred at time
                Amount = 1;
            else
            {
                if ((WordLen != S7Consts.S7WLCounter) && (WordLen != S7Consts.S7WLTimer))
                {
                    Amount = Amount * WordSize;
                    WordSize = 1;
                    WordLen = S7Consts.S7WLByte;
                }
            }

            MaxElements = (_PDULength - 35) / WordSize; // 35 = Reply telegram header
            TotElements = Amount;

            while ((TotElements > 0) && (_LastError == 0))
            {
                NumElements = TotElements;
                if (NumElements > MaxElements)
                    NumElements = MaxElements;

                DataSize = NumElements * WordSize;
                IsoSize = Size_WR + DataSize;

                // Setup the telegram
                Array.Copy(S7_RW, 0, PDU, 0, Size_WR);
                // Whole telegram Size
                S7.SetWordAt(PDU, 2, (ushort)IsoSize);
                // Data Length
                Length = DataSize + 4;
                S7.SetWordAt(PDU, 15, (ushort)Length);
                // Function
                PDU[17] = (byte)0x05;
                // Set DB Number
                PDU[27] = (byte)Area;
                if (Area == S7Consts.S7AreaDB)
                    S7.SetWordAt(PDU, 25, (ushort)DBNumber);


                // Adjusts Start and word length
                if ((WordLen == S7Consts.S7WLBit) || (WordLen == S7Consts.S7WLCounter) || (WordLen == S7Consts.S7WLTimer))
                {
                    Address = Start;
                    Length = DataSize;
                    PDU[22] = (byte)WordLen;
                }
                else
                {
                    Address = Start << 3;
                    Length = DataSize << 3;
                }

                // Num elements
                S7.SetWordAt(PDU, 23, (ushort)NumElements);
                // Address into the PLC
                PDU[30] = (byte)(Address & 0x0FF);
                Address = Address >> 8;
                PDU[29] = (byte)(Address & 0x0FF);
                Address = Address >> 8;
                PDU[28] = (byte)(Address & 0x0FF);

                // Transport Size
                switch (WordLen)
                {
                    case S7Consts.S7WLBit:
                        PDU[32] = TS_ResBit;
                        break;
                    case S7Consts.S7WLCounter:
                    case S7Consts.S7WLTimer:
                        PDU[32] = TS_ResOctet;
                        break;
                    default:
                        PDU[32] = TS_ResByte; // byte/word/dword etc.
                        break;
                }
                ;
                // Length
                S7.SetWordAt(PDU, 33, (ushort)Length);

                // Copies the Data
                Array.Copy(Buffer, Offset, PDU, 35, DataSize);

                SendPacket(PDU, IsoSize);
                if (_LastError == 0)
                {
                    Length = RecvIsoPacket();
                    if (_LastError == 0)
                    {
                        if (Length == 22)
                        {
                            if (PDU[21] != (byte)0xFF)
                                _LastError = CpuError(PDU[21]);
                        }
                        else
                            _LastError = S7Consts.errIsoInvalidPDU;
                    }
                }
                Offset += DataSize;
                TotElements -= NumElements;
                Start += NumElements * WordSize;
            }

            if (_LastError == 0)
            {
                BytesWritten = Offset;
                Time_ms = Environment.TickCount - Elapsed;
            }
            else
                BytesWritten = 0;

            return _LastError;
        }

        /// <summary>
        /// Asynchronously writes an area of the PLC with bytes written output.
        /// </summary>
        /// <param name="Area">Area code.</param>
        /// <param name="DBNumber">DB number.</param>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Amount to write.</param>
        /// <param name="WordLen">Word length.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <param name="BytesWritten">Reference to bytes written.</param>
        /// <returns>Error code.</returns>
        public async Task<int> WriteAreaAsync(int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer, int BytesWritten)
        {
            int localBytesWritten = BytesWritten;
            int result = await Task.Run(() => WriteArea(Area, DBNumber, Start, Amount, WordLen, Buffer, ref localBytesWritten));
            BytesWritten = localBytesWritten;
            return result;
        }

        /// <summary>
        /// Reads multiple variables from the PLC.
        /// </summary>
        /// <param name="Items">Array of S7DataItem to read.</param>
        /// <param name="ItemsCount">Number of items.</param>
        /// <returns>Error code.</returns>
        public int ReadMultiVars(S7DataItem[] Items, int ItemsCount)
        {
            int Offset;
            int Length;
            int ItemSize;
            byte[] S7Item = new byte[12];
            byte[] S7ItemRead = new byte[1024];

            _LastError = 0;
            Time_ms = 0;
            int Elapsed = Environment.TickCount;

            // Checks items
            if (ItemsCount > MaxVars)
                return S7Consts.errCliTooManyItems;

            // Fills Header
            Array.Copy(S7_MRD_HEADER, 0, PDU, 0, S7_MRD_HEADER.Length);
            S7.SetWordAt(PDU, 13, (ushort)(ItemsCount * S7Item.Length + 2));
            PDU[18] = (byte)ItemsCount;
            // Fills the Items
            Offset = 19;
            for (int c = 0; c < ItemsCount; c++)
            {
                Array.Copy(S7_MRD_ITEM, S7Item, S7Item.Length);
                S7Item[3] = (byte)Items[c].WordLen;
                S7.SetWordAt(S7Item, 4, (ushort)Items[c].Amount);
                if (Items[c].Area == S7Consts.S7AreaDB)
                    S7.SetWordAt(S7Item, 6, (ushort)Items[c].DBNumber);
                S7Item[8] = (byte)Items[c].Area;

                // Address into the PLC
                int Address = Items[c].Start;
                S7Item[11] = (byte)(Address & 0x0FF);
                Address = Address >> 8;
                S7Item[10] = (byte)(Address & 0x0FF);
                Address = Address >> 8;
                S7Item[09] = (byte)(Address & 0x0FF);

                Array.Copy(S7Item, 0, PDU, Offset, S7Item.Length);
                Offset += S7Item.Length;
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
                Array.Copy(PDU, Offset, S7ItemRead, 0, Length - Offset);
                if (S7ItemRead[0] == 0xff)
                {
                    ItemSize = (int)S7.GetWordAt(S7ItemRead, 2);
                    if ((S7ItemRead[1] != TS_ResOctet) && (S7ItemRead[1] != TS_ResReal) && (S7ItemRead[1] != TS_ResBit))
                        ItemSize = ItemSize >> 3;
                    Marshal.Copy(S7ItemRead, 4, Items[c].pData, ItemSize);
                    Items[c].Result = 0;
                    if (ItemSize % 2 != 0)
                        ItemSize++; // Odd size are rounded
                    Offset = Offset + 4 + ItemSize;
                }
                else
                {
                    Items[c].Result = CpuError(S7ItemRead[0]);
                    Offset += 4; // Skip the Item header                           
                }
            }
            Time_ms = Environment.TickCount - Elapsed;
            return _LastError;
        }

        /// <summary>
        /// Asynchronously reads multiple variables from the PLC.
        /// </summary>
        /// <param name="Items">Array of S7DataItem to read.</param>
        /// <param name="ItemsCount">Number of items.</param>
        /// <returns>Error code.</returns>
        public async Task<int> ReadMultiVarsAsync(S7DataItem[] Items, int ItemsCount)
        {
            return await Task.Run(() => ReadMultiVars(Items, ItemsCount));
        }

        /// <summary>
        /// Writes multiple variables to the PLC.
        /// </summary>
        /// <param name="Items">Array of S7DataItem to write.</param>
        /// <param name="ItemsCount">Number of items.</param>
        /// <returns>Error code.</returns>
        public int WriteMultiVars(S7DataItem[] Items, int ItemsCount)
        {
            int Offset;
            int ParLength;
            int DataLength;
            int ItemDataSize;
            byte[] S7ParItem = new byte[S7_MWR_PARAM.Length];
            byte[] S7DataItem = new byte[1024];

            _LastError = 0;
            Time_ms = 0;
            int Elapsed = Environment.TickCount;

            // Checks items
            if (ItemsCount > MaxVars)
                return S7Consts.errCliTooManyItems;
            // Fills Header
            Array.Copy(S7_MWR_HEADER, 0, PDU, 0, S7_MWR_HEADER.Length);
            ParLength = ItemsCount * S7_MWR_PARAM.Length + 2;
            S7.SetWordAt(PDU, 13, (ushort)ParLength);
            PDU[18] = (byte)ItemsCount;
            // Fills Params
            Offset = S7_MWR_HEADER.Length;
            for (int c = 0; c < ItemsCount; c++)
            {
                Array.Copy(S7_MWR_PARAM, 0, S7ParItem, 0, S7_MWR_PARAM.Length);
                S7ParItem[3] = (byte)Items[c].WordLen;
                S7ParItem[8] = (byte)Items[c].Area;
                S7.SetWordAt(S7ParItem, 4, (ushort)Items[c].Amount);
                S7.SetWordAt(S7ParItem, 6, (ushort)Items[c].DBNumber);
                // Address into the PLC
                int Address = Items[c].Start;
                S7ParItem[11] = (byte)(Address & 0x0FF);
                Address = Address >> 8;
                S7ParItem[10] = (byte)(Address & 0x0FF);
                Address = Address >> 8;
                S7ParItem[09] = (byte)(Address & 0x0FF);
                Array.Copy(S7ParItem, 0, PDU, Offset, S7ParItem.Length);
                Offset += S7_MWR_PARAM.Length;
            }
            // Fills Data
            DataLength = 0;
            for (int c = 0; c < ItemsCount; c++)
            {
                S7DataItem[0] = 0x00;
                switch (Items[c].WordLen)
                {
                    case S7Consts.S7WLBit:
                        S7DataItem[1] = TS_ResBit;
                        break;
                    case S7Consts.S7WLCounter:
                    case S7Consts.S7WLTimer:
                        S7DataItem[1] = TS_ResOctet;
                        break;
                    default:
                        S7DataItem[1] = TS_ResByte; // byte/word/dword etc.
                        break;
                }
                ;
                if ((Items[c].WordLen == S7Consts.S7WLTimer) || (Items[c].WordLen == S7Consts.S7WLCounter))
                    ItemDataSize = Items[c].Amount * 2;
                else
                    ItemDataSize = Items[c].Amount;

                if ((S7DataItem[1] != TS_ResOctet) && (S7DataItem[1] != TS_ResBit))
                    S7.SetWordAt(S7DataItem, 2, (ushort)(ItemDataSize * 8));
                else
                    S7.SetWordAt(S7DataItem, 2, (ushort)ItemDataSize);

                Marshal.Copy(Items[c].pData, S7DataItem, 4, ItemDataSize);
                if (ItemDataSize % 2 != 0)
                {
                    S7DataItem[ItemDataSize + 4] = 0x00;
                    ItemDataSize++;
                }
                Array.Copy(S7DataItem, 0, PDU, Offset, ItemDataSize + 4);
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

        /// <summary>
        /// Asynchronously writes multiple variables to the PLC.
        /// </summary>
        /// <param name="Items">Array of S7DataItem to write.</param>
        /// <param name="ItemsCount">Number of items.</param>
        /// <returns>Error code.</returns>
        public async Task<int> WriteMultiVarsAsync(S7DataItem[] Items, int ItemsCount)
        {
            return await Task.Run(() => WriteMultiVars(Items, ItemsCount));
        }

        #endregion
    }
}
