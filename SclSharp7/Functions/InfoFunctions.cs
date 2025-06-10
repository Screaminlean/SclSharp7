namespace SclSharp7
{
    public partial class S7Client
    {
        #region [Info Functions / Properties]

        /// <summary>
        /// Gets the error text for a given error code.
        /// </summary>
        /// <param name="Error">Error code.</param>
        /// <returns>Error text.</returns>
        public string ErrorText(int Error)
        {
            switch (Error)
            {
                case 0: return "OK";
                case S7Consts.errTCPSocketCreation: return "SYS : Error creating the Socket";
                case S7Consts.errTCPConnectionTimeout: return "TCP : Connection Timeout";
                case S7Consts.errTCPConnectionFailed: return "TCP : Connection Error";
                case S7Consts.errTCPReceiveTimeout: return "TCP : Data receive Timeout";
                case S7Consts.errTCPDataReceive: return "TCP : Error receiving Data";
                case S7Consts.errTCPSendTimeout: return "TCP : Data send Timeout";
                case S7Consts.errTCPDataSend: return "TCP : Error sending Data";
                case S7Consts.errTCPConnectionReset: return "TCP : Connection reset by the Peer";
                case S7Consts.errTCPNotConnected: return "CLI : Client not connected";
                case S7Consts.errTCPUnreachableHost: return "TCP : Unreachable host";
                case S7Consts.errIsoConnect: return "ISO : Connection Error";
                case S7Consts.errIsoInvalidPDU: return "ISO : Invalid PDU received";
                case S7Consts.errIsoInvalidDataSize: return "ISO : Invalid Buffer passed to Send/Receive";
                case S7Consts.errCliNegotiatingPDU: return "CLI : Error in PDU negotiation";
                case S7Consts.errCliInvalidParams: return "CLI : invalid param(s) supplied";
                case S7Consts.errCliJobPending: return "CLI : Job pending";
                case S7Consts.errCliTooManyItems: return "CLI : too may items (>20) in multi read/write";
                case S7Consts.errCliInvalidWordLen: return "CLI : invalid WordLength";
                case S7Consts.errCliPartialDataWritten: return "CLI : Partial data written";
                case S7Consts.errCliSizeOverPDU: return "CPU : total data exceeds the PDU size";
                case S7Consts.errCliInvalidPlcAnswer: return "CLI : invalid CPU answer";
                case S7Consts.errCliAddressOutOfRange: return "CPU : Address out of range";
                case S7Consts.errCliInvalidTransportSize: return "CPU : Invalid Transport size";
                case S7Consts.errCliWriteDataSizeMismatch: return "CPU : Data size mismatch";
                case S7Consts.errCliItemNotAvailable: return "CPU : Item not available";
                case S7Consts.errCliInvalidValue: return "CPU : Invalid value supplied";
                case S7Consts.errCliCannotStartPLC: return "CPU : Cannot start PLC";
                case S7Consts.errCliAlreadyRun: return "CPU : PLC already RUN";
                case S7Consts.errCliCannotStopPLC: return "CPU : Cannot stop PLC";
                case S7Consts.errCliCannotCopyRamToRom: return "CPU : Cannot copy RAM to ROM";
                case S7Consts.errCliCannotCompress: return "CPU : Cannot compress";
                case S7Consts.errCliAlreadyStop: return "CPU : PLC already STOP";
                case S7Consts.errCliFunNotAvailable: return "CPU : Function not available";
                case S7Consts.errCliUploadSequenceFailed: return "CPU : Upload sequence failed";
                case S7Consts.errCliInvalidDataSizeRecvd: return "CLI : Invalid data size received";
                case S7Consts.errCliInvalidBlockType: return "CLI : Invalid block type";
                case S7Consts.errCliInvalidBlockNumber: return "CLI : Invalid block number";
                case S7Consts.errCliInvalidBlockSize: return "CLI : Invalid block size";
                case S7Consts.errCliNeedPassword: return "CPU : Function not authorized for current protection level";
                case S7Consts.errCliInvalidPassword: return "CPU : Invalid password";
                case S7Consts.errCliNoPasswordToSetOrClear: return "CPU : No password to set or clear";
                case S7Consts.errCliJobTimeout: return "CLI : Job Timeout";
                case S7Consts.errCliFunctionRefused: return "CLI : function refused by CPU (Unknown error)";
                case S7Consts.errCliPartialDataRead: return "CLI : Partial data read";
                case S7Consts.errCliBufferTooSmall: return "CLI : The buffer supplied is too small to accomplish the operation";
                case S7Consts.errCliDestroying: return "CLI : Cannot perform (destroying)";
                case S7Consts.errCliInvalidParamNumber: return "CLI : Invalid Param Number";
                case S7Consts.errCliCannotChangeParam: return "CLI : Cannot change this param now";
                case S7Consts.errCliFunctionNotImplemented: return "CLI : Function not implemented";
                default: return "CLI : Unknown error (0x" + Convert.ToString(Error, 16) + ")";
            }
            ;
        }

        /// <summary>
        /// Asynchronously gets the error text for a given error code.
        /// </summary>
        /// <param name="Error">Error code.</param>
        /// <returns>Error text.</returns>
        public async Task<string> ErrorTextAsync(int Error)
        {
            return await Task.Run(() => ErrorText(Error));
        }

        /// <summary>
        /// Gets the last error code.
        /// </summary>
        /// <returns>Last error code.</returns>
        public int LastError()
        {
            return _LastError;
        }

        /// <summary>
        /// Asynchronously gets the last error code.
        /// </summary>
        /// <returns>Last error code.</returns>
        public async Task<int> LastErrorAsync()
        {
            return await Task.Run(() => LastError());
        }

        /// <summary>
        /// Gets the requested PDU length.
        /// </summary>
        /// <returns>Requested PDU length.</returns>
        public int RequestedPduLength()
        {
            return _PduSizeRequested;
        }

        /// <summary>
        /// Asynchronously gets the requested PDU length.
        /// </summary>
        /// <returns>Requested PDU length.</returns>
        public async Task<int> RequestedPduLengthAsync()
        {
            return await Task.Run(() => RequestedPduLength());
        }

        /// <summary>
        /// Gets the negotiated PDU length.
        /// </summary>
        /// <returns>Negotiated PDU length.</returns>
        public int NegotiatedPduLength()
        {
            return _PDULength;
        }

        /// <summary>
        /// Asynchronously gets the negotiated PDU length.
        /// </summary>
        /// <returns>Negotiated PDU length.</returns>
        public async Task<int> NegotiatedPduLengthAsync()
        {
            return await Task.Run(() => NegotiatedPduLength());
        }

        /// <summary>
        /// Gets the execution time of the last operation.
        /// </summary>
        /// <returns>Execution time in milliseconds.</returns>
        public int ExecTime()
        {
            return Time_ms;
        }

        /// <summary>
        /// Asynchronously gets the execution time of the last operation.
        /// </summary>
        /// <returns>Execution time in milliseconds.</returns>
        public async Task<int> ExecTimeAsync()
        {
            return await Task.Run(() => ExecTime());
        }

        /// <summary>
        /// Gets the execution time of the last operation (property).
        /// </summary>
        public int ExecutionTime
        {
            get
            {
                return Time_ms;
            }
        }

        /// <summary>
        /// Gets the negotiated PDU size (property).
        /// </summary>
        public int PduSizeNegotiated
        {
            get
            {
                return _PDULength;
            }
        }

        /// <summary>
        /// Gets or sets the requested PDU size (property).
        /// </summary>
        public int PduSizeRequested
        {
            get
            {
                return _PduSizeRequested;
            }
            set
            {
                if (value < MinPduSizeToRequest)
                    value = MinPduSizeToRequest;
                if (value > MaxPduSizeToRequest)
                    value = MaxPduSizeToRequest;
                _PduSizeRequested = value;
            }
        }

        /// <summary>
        /// Gets or sets the PLC port.
        /// </summary>
        public int PLCPort
        {
            get
            {
                return _PLCPort;
            }
            set
            {
                _PLCPort = value;
            }
        }

        /// <summary>
        /// Gets or sets the connection timeout.
        /// </summary>
        public int ConnTimeout
        {
            get
            {
                return _ConnTimeout;
            }
            set
            {
                _ConnTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the receive timeout.
        /// </summary>
        public int RecvTimeout
        {
            get
            {
                return _RecvTimeout;
            }
            set
            {
                _RecvTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the send timeout.
        /// </summary>
        public int SendTimeout
        {
            get
            {
                return _SendTimeout;
            }
            set
            {
                _SendTimeout = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the client is connected.
        /// </summary>
        public bool Connected
        {
            get
            {
                return (Socket != null) && (Socket.Connected);
            }
        }
        #endregion
    }
}
