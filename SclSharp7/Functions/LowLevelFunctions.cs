namespace SclSharp7
{
    public partial class S7Client
    {
        #region [Low Level]

        /// <summary>
        /// Exchanges an ISO buffer with the PLC.
        /// </summary>
        /// <param name="Buffer">Buffer to send and receive data.</param>
        /// <param name="Size">Reference to the size of the buffer. Updated with the received size.</param>
        /// <returns>Error code.</returns>
        public int IsoExchangeBuffer(byte[] Buffer, ref Int32 Size)
        {
            _LastError = 0;
            Time_ms = 0;
            int Elapsed = Environment.TickCount;
            Array.Copy(TPKT_ISO, 0, PDU, 0, TPKT_ISO.Length);
            S7.SetWordAt(PDU, 2, (ushort)(Size + TPKT_ISO.Length));
            try
            {
                Array.Copy(Buffer, 0, PDU, TPKT_ISO.Length, Size);
            }
            catch
            {
                return S7Consts.errIsoInvalidPDU;
            }
            SendPacket(PDU, TPKT_ISO.Length + Size);
            if (_LastError == 0)
            {
                int Length = RecvIsoPacket();
                if (_LastError == 0)
                {
                    Array.Copy(PDU, TPKT_ISO.Length, Buffer, 0, Length - TPKT_ISO.Length);
                    Size = Length - TPKT_ISO.Length;
                }
            }
            if (_LastError == 0)
                Time_ms = Environment.TickCount - Elapsed;
            else
                Size = 0;
            return _LastError;
        }

        /// <summary>
        /// Asynchronously exchanges an ISO buffer with the PLC.
        /// </summary>
        /// <param name="Buffer">Buffer to send and receive data.</param>
        /// <param name="Size">Reference to the size of the buffer. Updated with the received size.</param>
        /// <returns>Error code.</returns>
        public async Task<int> IsoExchangeBufferAsync(byte[] Buffer, int Size)
        {
            int localSize = Size;
            int result = await Task.Run(() => IsoExchangeBuffer(Buffer, ref localSize));
            Size = localSize;
            return result;
        }

        #endregion
    }
}
