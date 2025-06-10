namespace SclSharp7
{
    public partial class S7Client
    {
        #region [Control functions]

        /// <summary>
        /// Performs a hot start of the PLC.
        /// </summary>
        /// <returns>Error code.</returns>
        public int PlcHotStart()
        {
            _LastError = 0;
            int Elapsed = Environment.TickCount;

            SendPacket(S7_HOT_START);
            if (_LastError == 0)
            {
                int Length = RecvIsoPacket();
                if (Length > 18) // 18 is the minimum expected
                {
                    if (PDU[19] != pduStart)
                        _LastError = S7Consts.errCliCannotStartPLC;
                    else
                    {
                        if (PDU[20] == pduAlreadyStarted)
                            _LastError = S7Consts.errCliAlreadyRun;
                        else
                            _LastError = S7Consts.errCliCannotStartPLC;
                    }
                }
                else
                    _LastError = S7Consts.errIsoInvalidPDU;
            }
            if (_LastError == 0)
                Time_ms = Environment.TickCount - Elapsed;
            return _LastError;
        }

        /// <summary>
        /// Asynchronously performs a hot start of the PLC by calling PlcHotStart.
        /// </summary>
        /// <returns>Error code.</returns>
        public async Task<int> PlcHotStartAsync()
        {
            return await Task.Run(() => PlcHotStart());
        }

        /// <summary>
        /// Performs a cold start of the PLC.
        /// </summary>
        /// <returns>Error code.</returns>
        public int PlcColdStart()
        {
            _LastError = 0;
            int Elapsed = Environment.TickCount;

            SendPacket(S7_COLD_START);
            if (_LastError == 0)
            {
                int Length = RecvIsoPacket();
                if (Length > 18) // 18 is the minimum expected
                {
                    if (PDU[19] != pduStart)
                        _LastError = S7Consts.errCliCannotStartPLC;
                    else
                    {
                        if (PDU[20] == pduAlreadyStarted)
                            _LastError = S7Consts.errCliAlreadyRun;
                        else
                            _LastError = S7Consts.errCliCannotStartPLC;
                    }
                }
                else
                    _LastError = S7Consts.errIsoInvalidPDU;
            }
            if (_LastError == 0)
                Time_ms = Environment.TickCount - Elapsed;
            return _LastError;
        }

        /// <summary>
        /// Asynchronously performs a cold start of the PLC by calling PlcColdStart.
        /// </summary>
        /// <returns>Error code.</returns>
        public async Task<int> PlcColdStartAsync()
        {
            return await Task.Run(() => PlcColdStart());
        }

        /// <summary>
        /// Stops the PLC.
        /// </summary>
        /// <returns>Error code.</returns>
        public int PlcStop()
        {
            _LastError = 0;
            int Elapsed = Environment.TickCount;

            SendPacket(S7_STOP);
            if (_LastError == 0)
            {
                int Length = RecvIsoPacket();
                if (Length > 18) // 18 is the minimum expected
                {
                    if (PDU[19] != pduStop)
                        _LastError = S7Consts.errCliCannotStopPLC;
                    else
                    {
                        if (PDU[20] == pduAlreadyStopped)
                            _LastError = S7Consts.errCliAlreadyStop;
                        else
                            _LastError = S7Consts.errCliCannotStopPLC;
                    }
                }
                else
                    _LastError = S7Consts.errIsoInvalidPDU;
            }
            if (_LastError == 0)
                Time_ms = Environment.TickCount - Elapsed;
            return _LastError;
        }

        /// <summary>
        /// Asynchronously stops the PLC by calling PlcStop.
        /// </summary>
        /// <returns>Error code.</returns>
        public async Task<int> PlcStopAsync()
        {
            return await Task.Run(() => PlcStop());
        }

        /// <summary>
        /// Copies the PLC RAM to ROM. Not implemented.
        /// </summary>
        /// <param name="Timeout">Timeout value.</param>
        /// <returns>Error code.</returns>
        public int PlcCopyRamToRom(UInt32 Timeout)
        {
            return S7Consts.errCliFunctionNotImplemented;
        }

        /// <summary>
        /// Asynchronously copies the PLC RAM to ROM by calling PlcCopyRamToRom.
        /// </summary>
        /// <param name="Timeout">Timeout value.</param>
        /// <returns>Error code.</returns>
        public async Task<int> PlcCopyRamToRomAsync(UInt32 Timeout)
        {
            return await Task.Run(() => PlcCopyRamToRom(Timeout));
        }

        /// <summary>
        /// Compresses the PLC memory. Not implemented.
        /// </summary>
        /// <param name="Timeout">Timeout value.</param>
        /// <returns>Error code.</returns>
        public int PlcCompress(UInt32 Timeout)
        {
            return S7Consts.errCliFunctionNotImplemented;
        }

        /// <summary>
        /// Asynchronously compresses the PLC memory by calling PlcCompress.
        /// </summary>
        /// <param name="Timeout">Timeout value.</param>
        /// <returns>Error code.</returns>
        public async Task<int> PlcCompressAsync(UInt32 Timeout)
        {
            return await Task.Run(() => PlcCompress(Timeout));
        }

        /// <summary>
        /// Gets the current status of the PLC CPU.
        /// </summary>
        /// <param name="Status">Reference to an integer that receives the status code.</param>
        /// <returns>Error code.</returns>
        public int PlcGetStatus(ref Int32 Status)
        {
            _LastError = 0;
            int Elapsed = Environment.TickCount;

            SendPacket(S7_GET_STAT);
            if (_LastError == 0)
            {
                int Length = RecvIsoPacket();
                if (Length > 30) // the minimum expected
                {
                    ushort Result = S7.GetWordAt(PDU, 27);
                    if (Result == 0)
                    {
                        switch (PDU[44])
                        {
                            case S7Consts.S7CpuStatusUnknown:
                            case S7Consts.S7CpuStatusRun:
                            case S7Consts.S7CpuStatusStop:
                                {
                                    Status = PDU[44];
                                    break;
                                }
                            default:
                                {
                                    // Since RUN status is always 0x08 for all CPUs and CPs, STOP status
                                    // sometime can be coded as 0x03 (especially for old cpu...)
                                    Status = S7Consts.S7CpuStatusStop;
                                    break;
                                }
                        }
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
        /// Asynchronously gets the current status of the PLC CPU by calling PlcGetStatus.
        /// </summary>
        /// <param name="Status">Reference to an integer that receives the status code.</param>
        /// <returns>Error code.</returns>
        public async Task<int> PlcGetStatusAsync(Int32 Status)
        {
            int localStatus = Status;
            int result = await Task.Run(() => PlcGetStatus(ref localStatus));
            Status = localStatus;
            return result;
        }

        #endregion
    }
}
