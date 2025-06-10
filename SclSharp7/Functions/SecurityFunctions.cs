namespace SclSharp7
{
    public partial class S7Client
    {
        #region [Security functions]
        /// <summary>
        /// Sets the session password for the PLC.
        /// </summary>
        /// <param name="Password">Password to set.</param>
        /// <returns>Error code.</returns>
        public int SetSessionPassword(string Password)
        {
            byte[] pwd = { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
            int Length;
            _LastError = 0;
            int Elapsed = Environment.TickCount;
            // Encodes the Password
            S7.SetCharsAt(pwd, 0, Password);
            pwd[0] = (byte)(pwd[0] ^ 0x55);
            pwd[1] = (byte)(pwd[1] ^ 0x55);
            for (int c = 2; c < 8; c++)
            {
                pwd[c] = (byte)(pwd[c] ^ 0x55 ^ pwd[c - 2]);
            }
            Array.Copy(pwd, 0, S7_SET_PWD, 29, 8);
            // Sends the telegrem
            SendPacket(S7_SET_PWD);
            if (_LastError == 0)
            {
                Length = RecvIsoPacket();
                if (Length > 32) // the minimum expected
                {
                    ushort Result = S7.GetWordAt(PDU, 27);
                    if (Result != 0)
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
        /// Asynchronously sets the session password for the PLC.
        /// </summary>
        /// <param name="Password">Password to set.</param>
        /// <returns>Error code.</returns>
        public async Task<int> SetSessionPasswordAsync(string Password)
        {
            return await Task.Run(() => SetSessionPassword(Password));
        }

        /// <summary>
        /// Clears the session password for the PLC.
        /// </summary>
        /// <returns>Error code.</returns>
        public int ClearSessionPassword()
        {
            int Length;
            _LastError = 0;
            int Elapsed = Environment.TickCount;
            SendPacket(S7_CLR_PWD);
            if (_LastError == 0)
            {
                Length = RecvIsoPacket();
                if (Length > 30) // the minimum expected
                {
                    ushort Result = S7.GetWordAt(PDU, 27);
                    if (Result != 0)
                        _LastError = CpuError(Result);
                }
                else
                    _LastError = S7Consts.errIsoInvalidPDU;
            }
            return _LastError;
        }

        /// <summary>
        /// Asynchronously clears the session password for the PLC.
        /// </summary>
        /// <returns>Error code.</returns>
        public async Task<int> ClearSessionPasswordAsync()
        {
            return await Task.Run(() => ClearSessionPassword());
        }

        /// <summary>
        /// Gets the protection settings from the PLC.
        /// </summary>
        /// <param name="Protection">Reference to S7Protection to receive the protection settings.</param>
        /// <returns>Error code.</returns>
        public int GetProtection(ref S7Protection Protection)
        {
            S7Client.S7SZL SZL = new S7Client.S7SZL();
            int Size = 256;
            SZL.Data = new byte[Size];
            _LastError = ReadSZL(0x0232, 0x0004, ref SZL, ref Size);
            if (_LastError == 0)
            {
                Protection.sch_schal = S7.GetWordAt(SZL.Data, 2);
                Protection.sch_par = S7.GetWordAt(SZL.Data, 4);
                Protection.sch_rel = S7.GetWordAt(SZL.Data, 6);
                Protection.bart_sch = S7.GetWordAt(SZL.Data, 8);
                Protection.anl_sch = S7.GetWordAt(SZL.Data, 10);
            }
            return _LastError;
        }

        /// <summary>
        /// Asynchronously gets the protection settings from the PLC.
        /// </summary>
        /// <param name="Protection">Reference to S7Protection to receive the protection settings.</param>
        /// <returns>Error code.</returns>
        public async Task<int> GetProtectionAsync(S7Protection Protection)
        {
            S7Protection localProtection = Protection;
            int result = await Task.Run(() => GetProtection(ref localProtection));
            Protection = localProtection;
            return result;
        }
        #endregion
    }
}
