using System.Threading.Tasks;

namespace SclSharp7
{
    public partial class S7Client
    {
        #region [Blocks functions]

        /// <summary>
        /// Uploads a block from the PLC. Not implemented.
        /// </summary>
        /// <param name="BlockType">Type of the block.</param>
        /// <param name="BlockNum">Block number.</param>
        /// <param name="UsrData">User data buffer.</param>
        /// <param name="Size">Reference to the size of the buffer.</param>
        /// <returns>Error code.</returns>
        public int Upload(int BlockType, int BlockNum, byte[] UsrData, ref int Size)
        {
            return S7Consts.errCliFunctionNotImplemented;
        }

        /// <summary>
        /// Asynchronously uploads a block from the PLC. Not implemented.
        /// </summary>
        /// <param name="BlockType">Type of the block.</param>
        /// <param name="BlockNum">Block number.</param>
        /// <param name="UsrData">User data buffer.</param>
        /// <param name="size">Size of the buffer.</param>
        /// <returns>Error code.</returns>
        public async Task<int> UploadAsync(int BlockType, int BlockNum, byte[] UsrData, int size)
        {
            // This is a placeholder async implementation matching the sync Upload stub.
            // Replace with real async logic if/when Upload is implemented.
            await Task.Yield();
            return S7Consts.errCliFunctionNotImplemented;
        }

        /// <summary>
        /// Uploads a full block from the PLC. Not implemented.
        /// </summary>
        /// <param name="BlockType">Type of the block.</param>
        /// <param name="BlockNum">Block number.</param>
        /// <param name="UsrData">User data buffer.</param>
        /// <param name="Size">Reference to the size of the buffer.</param>
        /// <returns>Error code.</returns>
        public int FullUpload(int BlockType, int BlockNum, byte[] UsrData, ref int Size)
        {
            return S7Consts.errCliFunctionNotImplemented;
        }

        /// <summary>
        /// Asynchronously uploads a full block from the PLC. Not implemented.
        /// </summary>
        /// <param name="BlockType">Type of the block.</param>
        /// <param name="BlockNum">Block number.</param>
        /// <param name="UsrData">User data buffer.</param>
        /// <param name="size">Size of the buffer.</param>
        /// <returns>Error code.</returns>
        public async Task<int> FullUploadAsync(int BlockType, int BlockNum, byte[] UsrData, int size)
        {
            // This is a placeholder async implementation matching the sync FullUpload stub.
            // Replace with real async logic if/when FullUpload is implemented.
            await Task.Yield();
            return S7Consts.errCliFunctionNotImplemented;
        }

        /// <summary>
        /// Downloads a block to the PLC. Not implemented.
        /// </summary>
        /// <param name="BlockNum">Block number.</param>
        /// <param name="UsrData">User data buffer.</param>
        /// <param name="Size">Size of the buffer.</param>
        /// <returns>Error code.</returns>
        public int Download(int BlockNum, byte[] UsrData, int Size)
        {
            return S7Consts.errCliFunctionNotImplemented;
        }

        /// <summary>
        /// Asynchronously downloads a block to the PLC. Not implemented.
        /// </summary>
        /// <param name="BlockNum">Block number.</param>
        /// <param name="UsrData">User data buffer.</param>
        /// <param name="Size">Size of the buffer.</param>
        /// <returns>Error code.</returns>
        public async Task<int> DownloadloadAsync(int BlockNum, byte[] UsrData, int Size)
        {
            // This is a placeholder async implementation matching the sync Downloadload stub.
            // Replace with real async logic if/when Downloadload is implemented.
            await Task.Yield();
            return S7Consts.errCliFunctionNotImplemented;
        }

        /// <summary>
        /// Deletes a block from the PLC. Not implemented.
        /// </summary>
        /// <param name="BlockType">Type of the block.</param>
        /// <param name="BlockNum">Block number.</param>
        /// <returns>Error code.</returns>
        public int Delete(int BlockType, int BlockNum)
        {
            return S7Consts.errCliFunctionNotImplemented;
        }

        /// <summary>
        /// Asynchronously deletes a block from the PLC. Not implemented.
        /// </summary>
        /// <param name="BlockType">Type of the block.</param>
        /// <param name="BlockNum">Block number.</param>
        /// <returns>Error code.</returns>
        public async Task<int> DeletedAsync(int BlockType, int BlockNum)
        {
            // This is a placeholder async implementation matching the sync Deleted stub.
            // Replace with real async logic if/when Deleted is implemented.
            await Task.Yield();
            return S7Consts.errCliFunctionNotImplemented;
        }

        /// <summary>
        /// Reads a data block (DB) from the PLC into the provided buffer.
        /// </summary>
        /// <param name="DBNumber">DB number to read.</param>
        /// <param name="UsrData">User data buffer to fill.</param>
        /// <param name="Size">Reference to the size of the buffer. Updated with the actual size read.</param>
        /// <returns>Error code.</returns>
        public int DBGet(int DBNumber, byte[] UsrData, ref int Size)
        {
            S7BlockInfo BI = new S7BlockInfo();
            int Elapsed = Environment.TickCount;
            Time_ms = 0;

            _LastError = GetAgBlockInfo(Block_DB, DBNumber, ref BI);

            if (_LastError == 0)
            {
                int DBSize = BI.MC7Size;
                if (DBSize <= UsrData.Length)
                {
                    Size = DBSize;
                    _LastError = DBRead(DBNumber, 0, DBSize, UsrData);
                    if (_LastError == 0)
                        Size = DBSize;
                }
                else
                    _LastError = S7Consts.errCliBufferTooSmall;
            }
            if (_LastError == 0)
                Time_ms = Environment.TickCount - Elapsed;
            return _LastError;
        }

        /// <summary>
        /// Asynchronously reads a data block (DB) from the PLC into the provided buffer.
        /// </summary>
        /// <param name="DBNumber">DB number to read.</param>
        /// <param name="UsrData">User data buffer to fill.</param>
        /// <param name="size">Size of the buffer.</param>
        /// <returns>Error code.</returns>
        public async Task<int> DBGetAsync(int DBNumber, byte[] UsrData, int size)
        {
            int _LastError = await Task.Run(() => DBGet(DBNumber, UsrData, ref size));
            return _LastError;
        }

        /// <summary>
        /// Fills a data block (DB) in the PLC with a specified value.
        /// </summary>
        /// <param name="DBNumber">DB number to fill.</param>
        /// <param name="FillChar">Value to fill the DB with.</param>
        /// <returns>Error code.</returns>
        public int DBFill(int DBNumber, int FillChar)
        {
            S7BlockInfo BI = new S7BlockInfo();
            int Elapsed = Environment.TickCount;
            Time_ms = 0;

            _LastError = GetAgBlockInfo(Block_DB, DBNumber, ref BI);

            if (_LastError == 0)
            {
                byte[] Buffer = new byte[BI.MC7Size];
                for (int c = 0; c < BI.MC7Size; c++)
                    Buffer[c] = (byte)FillChar;
                _LastError = DBWrite(DBNumber, 0, BI.MC7Size, Buffer);
            }
            if (_LastError == 0)
                Time_ms = Environment.TickCount - Elapsed;
            return _LastError;
        }

        /// <summary>
        /// Asynchronously fills a data block (DB) in the PLC with a specified value.
        /// </summary>
        /// <param name="DBNumber">DB number to fill.</param>
        /// <param name="FillChar">Value to fill the DB with.</param>
        /// <returns>Error code.</returns>
        public async Task<int> DBFillAsync(int DBNumber, int FillChar)
        {
            return await Task.Run(() => DBFill(DBNumber, FillChar));
        }

        #endregion
    }
}
