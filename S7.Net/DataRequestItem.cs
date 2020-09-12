
using S7.Net.Types;

namespace S7.Net
{
    internal class DataRequestItem
    {
        public DataRequestItem(DataType dataType, int db, int startByteAddress, int byteLength)
        {
            DataType = dataType;
            DB = db;
            StartByteAddress = startByteAddress;
            ByteLength = byteLength;
        }

        public DataRequestItem(DataItem dataItem)
            : this(dataItem.DataType, dataItem.DB, dataItem.StartByteAdr, Plc.VarTypeToByteLength(dataItem.VarType, dataItem.Count))
        {

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
