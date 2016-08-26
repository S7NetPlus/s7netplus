namespace S7.Net.Types
{
    /// <summary>
    /// Create an instance of a memory block that can be read by using ReadMultipleVars
    /// </summary>
    public class DataItem
    {
        /// <summary>
        /// Memory area to read 
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// Type of data to be read (default is bytes)
        /// </summary>
        public VarType VarType { get; set; }

        /// <summary>
        /// Address of memory area to read (example: for DB1 this value is 1, for T45 this value is 45)
        /// </summary>
        public int DB { get; set; }

        /// <summary>
        /// Address of the first byte to read
        /// </summary>
        public int StartByteAdr { get; set; }

        /// <summary>
        /// Number of variables to read
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Contains the value of the memory area after the read has been executed
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Create an instance of DataItem
        /// </summary>
        public DataItem()
        {
            VarType = VarType.Byte;
            Count = 1;
        }
    }
}
