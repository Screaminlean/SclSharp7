namespace SclSharp7
{
    public partial class S7Client
    {
        #region [Date/Time functions]

        /// <summary>
        /// Gets the PLC date and time.
        /// </summary>
        /// <param name="DT">Reference to a DateTime to receive the PLC date and time.</param>
        /// <returns>Error code.</returns>
        public int GetPlcDateTime(ref DateTime DT)
        {
            int Length;
            _LastError = 0;
            Time_ms = 0;
            int Elapsed = Environment.TickCount;

            SendPacket(S7_GET_DT);
            if (_LastError == 0)
            {
                Length = RecvIsoPacket();
                if (Length > 30) // the minimum expected
                {
                    if ((S7.GetWordAt(PDU, 27) == 0) && (PDU[29] == 0xFF))
                    {
                        DT = S7.GetDateTimeAt(PDU, 35);
                    }
                    else
                        _LastError = S7Consts.errCliInvalidPlcAnswer;
                }
                else
                    _LastError = S7Consts.errIsoInvalidPDU;
            }

            if (_LastError == 0)
                Time_ms = Environment.TickCount - Elapsed;

            return _LastError;
        }

        /// <summary>
        /// Asynchronously gets the PLC date and time.
        /// </summary>
        /// <param name="DT">Reference to a DateTime to receive the PLC date and time.</param>
        /// <returns>Error code.</returns>
        public async Task<int> GetPlcDateTimeAsync(DateTime DT)
        {
            DateTime localDT = DT;
            int result = await Task.Run(() => GetPlcDateTime(ref localDT));
            DT = localDT;
            return result;
        }

        /// <summary>
        /// Sets the PLC date and time.
        /// </summary>
        /// <param name="DT">DateTime to set in the PLC.</param>
        /// <returns>Error code.</returns>
        public int SetPlcDateTime(DateTime DT)
        {
            int Length;
            _LastError = 0;
            Time_ms = 0;
            int Elapsed = Environment.TickCount;

            S7.SetDateTimeAt(S7_SET_DT, 31, DT);
            SendPacket(S7_SET_DT);
            if (_LastError == 0)
            {
                Length = RecvIsoPacket();
                if (Length > 30) // the minimum expected
                {
                    if (S7.GetWordAt(PDU, 27) != 0)
                        _LastError = S7Consts.errCliInvalidPlcAnswer;
                }
                else
                    _LastError = S7Consts.errIsoInvalidPDU;
            }
            if (_LastError == 0)
                Time_ms = Environment.TickCount - Elapsed;

            return _LastError;
        }

        /// <summary>
        /// Asynchronously sets the PLC date and time.
        /// </summary>
        /// <param name="DT">DateTime to set in the PLC.</param>
        /// <returns>Error code.</returns>
        public async Task<int> SetPlcDateTimeAsync(DateTime DT)
        {
            return await Task.Run(() => SetPlcDateTime(DT));
        }

        /// <summary>
        /// Sets the PLC date and time to the system's current date and time.
        /// </summary>
        /// <returns>Error code.</returns>
        public int SetPlcSystemDateTime()
        {
            return SetPlcDateTime(DateTime.Now);
        }

        /// <summary>
        /// Asynchronously sets the PLC date and time to the system's current date and time.
        /// </summary>
        /// <returns>Error code.</returns>
        public async Task<int> SetPlcSystemDateTimeAsync()
        {
            return await Task.Run(() => SetPlcSystemDateTime());
        }

        #endregion
    }
}
