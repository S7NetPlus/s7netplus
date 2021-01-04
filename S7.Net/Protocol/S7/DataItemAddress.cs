namespace S7.Net.Protocol.S7
{
    /// <summary>
    /// Represents an area of memory in the PLC
    /// </summary>
    internal class DataItemAddress
    {
        public DataItemAddress(DataType dataType, int db, int startByteAddress, int byteLength)
        {
            DataType = dataType;
            DB = db;
            StartByteAddress = startByteAddress;
            ByteLength = byteLength;
        }


        /// <summary>
        /// Memory area to read 
        /// </summary>
        public DataType DataType { get; }

        /// <summary>
        /// Address of memory area to read (example: for DB1 this value is 1, for T45 this value is 45)
        /// </summary>
        public int DB { get; }

        /// <summary>
        /// Address of the first byte to read
        /// </summary>
        public int StartByteAddress { get; }

        /// <summary>
        /// Length of data to read
        /// </summary>
        public int ByteLength { get; }
    }
}
