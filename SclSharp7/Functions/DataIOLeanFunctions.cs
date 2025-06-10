namespace SclSharp7
{
    public partial class S7Client
    {
        #region [Data I/O lean functions]

        /// <summary>
        /// Reads from a DB area.
        /// </summary>
        /// <param name="DBNumber">DB number.</param>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to read.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <returns>Error code.</returns>
        public int DBRead(int DBNumber, int Start, int Size, byte[] Buffer)
        {
            return ReadArea(S7Consts.S7AreaDB, DBNumber, Start, Size, S7Consts.S7WLByte, Buffer);
        }

        /// <summary>
        /// Asynchronously reads from a DB area.
        /// </summary>
        /// <param name="DBNumber">DB number.</param>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to read.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <returns>Error code.</returns>
        public async Task<int> DBReadAsync(int DBNumber, int Start, int Size, byte[] Buffer)
        {
            return await Task.Run(() => DBRead(DBNumber, Start, Size, Buffer));
        }

        /// <summary>
        /// Writes to a DB area.
        /// </summary>
        /// <param name="DBNumber">DB number.</param>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to write.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <returns>Error code.</returns>
        public int DBWrite(int DBNumber, int Start, int Size, byte[] Buffer)
        {
            return WriteArea(S7Consts.S7AreaDB, DBNumber, Start, Size, S7Consts.S7WLByte, Buffer);
        }

        /// <summary>
        /// Asynchronously writes to a DB area.
        /// </summary>
        /// <param name="DBNumber">DB number.</param>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to write.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <returns>Error code.</returns>
        public async Task<int> DBWriteAsync(int DBNumber, int Start, int Size, byte[] Buffer)
        {
            return await Task.Run(() => DBWrite(DBNumber, Start, Size, Buffer));
        }

        /// <summary>
        /// Reads from the memory byte area.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to read.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <returns>Error code.</returns>
        public int MBRead(int Start, int Size, byte[] Buffer)
        {
            return ReadArea(S7Consts.S7AreaMK, 0, Start, Size, S7Consts.S7WLByte, Buffer);
        }

        /// <summary>
        /// Asynchronously reads from the memory byte area.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to read.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <returns>Error code.</returns>
        public async Task<int> MBReadAsync(int Start, int Size, byte[] Buffer)
        {
            return await Task.Run(() => MBRead(Start, Size, Buffer));
        }

        /// <summary>
        /// Writes to the memory byte area.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to write.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <returns>Error code.</returns>
        public int MBWrite(int Start, int Size, byte[] Buffer)
        {
            return WriteArea(S7Consts.S7AreaMK, 0, Start, Size, S7Consts.S7WLByte, Buffer);
        }

        /// <summary>
        /// Asynchronously writes to the memory byte area.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to write.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <returns>Error code.</returns>
        public async Task<int> MBWriteAsync(int Start, int Size, byte[] Buffer)
        {
            return await Task.Run(() => MBWrite(Start, Size, Buffer));
        }

        /// <summary>
        /// Reads from the input byte area.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to read.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <returns>Error code.</returns>
        public int EBRead(int Start, int Size, byte[] Buffer)
        {
            return ReadArea(S7Consts.S7AreaPE, 0, Start, Size, S7Consts.S7WLByte, Buffer);
        }

        /// <summary>
        /// Asynchronously reads from the input byte area.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to read.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <returns>Error code.</returns>
        public async Task<int> EBReadAsync(int Start, int Size, byte[] Buffer)
        {
            return await Task.Run(() => EBRead(Start, Size, Buffer));
        }

        /// <summary>
        /// Writes to the input byte area.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to write.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <returns>Error code.</returns>
        public int EBWrite(int Start, int Size, byte[] Buffer)
        {
            return WriteArea(S7Consts.S7AreaPE, 0, Start, Size, S7Consts.S7WLByte, Buffer);
        }

        /// <summary>
        /// Asynchronously writes to the input byte area.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to write.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <returns>Error code.</returns>
        public async Task<int> EBWriteAsync(int Start, int Size, byte[] Buffer)
        {
            return await Task.Run(() => EBWrite(Start, Size, Buffer));
        }

        /// <summary>
        /// Reads from the output byte area.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to read.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <returns>Error code.</returns>
        public int ABRead(int Start, int Size, byte[] Buffer)
        {
            return ReadArea(S7Consts.S7AreaPA, 0, Start, Size, S7Consts.S7WLByte, Buffer);
        }

        /// <summary>
        /// Asynchronously reads from the output byte area.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to read.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <returns>Error code.</returns>
        public async Task<int> ABReadAsync(int Start, int Size, byte[] Buffer)
        {
            return await Task.Run(() => ABRead(Start, Size, Buffer));
        }

        /// <summary>
        /// Writes to the output byte area.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to write.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <returns>Error code.</returns>
        public int ABWrite(int Start, int Size, byte[] Buffer)
        {
            return WriteArea(S7Consts.S7AreaPA, 0, Start, Size, S7Consts.S7WLByte, Buffer);
        }

        /// <summary>
        /// Asynchronously writes to the output byte area.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Size">Number of bytes to write.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <returns>Error code.</returns>
        public async Task<int> ABWriteAsync(int Start, int Size, byte[] Buffer)
        {
            return await Task.Run(() => ABWrite(Start, Size, Buffer));
        }

        /// <summary>
        /// Reads timers from the PLC.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Number of timers to read.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <returns>Error code.</returns>
        public int TMRead(int Start, int Amount, ushort[] Buffer)
        {
            byte[] sBuffer = new byte[Amount * 2];
            int Result = ReadArea(S7Consts.S7AreaTM, 0, Start, Amount, S7Consts.S7WLTimer, sBuffer);
            if (Result == 0)
            {
                for (int c = 0; c < Amount; c++)
                {
                    Buffer[c] = (ushort)((sBuffer[c * 2 + 1] << 8) + (sBuffer[c * 2]));
                }
            }
            return Result;
        }

        /// <summary>
        /// Asynchronously reads timers from the PLC.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Number of timers to read.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <returns>Error code.</returns>
        public async Task<int> TMReadAsync(int Start, int Amount, ushort[] Buffer)
        {
            return await Task.Run(() => TMRead(Start, Amount, Buffer));
        }

        /// <summary>
        /// Writes timers to the PLC.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Number of timers to write.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <returns>Error code.</returns>
        public int TMWrite(int Start, int Amount, ushort[] Buffer)
        {
            byte[] sBuffer = new byte[Amount * 2];
            for (int c = 0; c < Amount; c++)
            {
                sBuffer[c * 2 + 1] = (byte)((Buffer[c] & 0xFF00) >> 8);
                sBuffer[c * 2] = (byte)(Buffer[c] & 0x00FF);
            }
            return WriteArea(S7Consts.S7AreaTM, 0, Start, Amount, S7Consts.S7WLTimer, sBuffer);
        }

        /// <summary>
        /// Asynchronously writes timers to the PLC.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Number of timers to write.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <returns>Error code.</returns>
        public async Task<int> TMWriteAsync(int Start, int Amount, ushort[] Buffer)
        {
            return await Task.Run(() => TMWrite(Start, Amount, Buffer));
        }

        /// <summary>
        /// Reads counters from the PLC.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Number of counters to read.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <returns>Error code.</returns>
        public int CTRead(int Start, int Amount, ushort[] Buffer)
        {
            byte[] sBuffer = new byte[Amount * 2];
            int Result = ReadArea(S7Consts.S7AreaCT, 0, Start, Amount, S7Consts.S7WLCounter, sBuffer);
            if (Result == 0)
            {
                for (int c = 0; c < Amount; c++)
                {
                    Buffer[c] = (ushort)((sBuffer[c * 2 + 1] << 8) + (sBuffer[c * 2]));
                }
            }
            return Result;
        }

        /// <summary>
        /// Asynchronously reads counters from the PLC.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Number of counters to read.</param>
        /// <param name="Buffer">Buffer to fill.</param>
        /// <returns>Error code.</returns>
        public async Task<int> CTReadAsync(int Start, int Amount, ushort[] Buffer)
        {
            return await Task.Run(() => CTRead(Start, Amount, Buffer));
        }

        /// <summary>
        /// Writes counters to the PLC.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Number of counters to write.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <returns>Error code.</returns>
        public int CTWrite(int Start, int Amount, ushort[] Buffer)
        {
            byte[] sBuffer = new byte[Amount * 2];
            for (int c = 0; c < Amount; c++)
            {
                sBuffer[c * 2 + 1] = (byte)((Buffer[c] & 0xFF00) >> 8);
                sBuffer[c * 2] = (byte)(Buffer[c] & 0x00FF);
            }
            return WriteArea(S7Consts.S7AreaCT, 0, Start, Amount, S7Consts.S7WLCounter, sBuffer);
        }

        /// <summary>
        /// Asynchronously writes counters to the PLC.
        /// </summary>
        /// <param name="Start">Start address.</param>
        /// <param name="Amount">Number of counters to write.</param>
        /// <param name="Buffer">Buffer to write from.</param>
        /// <returns>Error code.</returns>
        public async Task<int> CTWriteAsync(int Start, int Amount, ushort[] Buffer)
        {
            return await Task.Run(() => CTWrite(Start, Amount, Buffer));
        }

        #endregion
    }
}
