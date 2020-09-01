using System;

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
        /// Addess of bit to read from StartByteAdr
        /// </summary>
        public byte BitAdr { get; set; }

        /// <summary>
        /// Number of variables to read
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Contains the value of the memory area after the read has been executed
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Create an instance of DataItem
        /// </summary>
        public DataItem()
        {
            VarType = VarType.Byte;
            Count = 1;
        }

        /// <summary>
        /// Create an instance of <see cref="DataItem"/> from the supplied address.
        /// </summary>
        /// <param name="address">The address to create the DataItem for.</param>
        /// <returns>A new <see cref="DataItem"/> instance with properties parsed from <paramref name="address"/>.</returns>
        /// <remarks>The <see cref="Count" /> property is not parsed from the address.</remarks>
        public static DataItem FromAddress(string address)
        {
            PLCAddress.Parse(address, out var dataType, out var dbNumber, out var varType, out var startByte,
                out var bitNumber);

            return new DataItem
            {
                DataType = dataType,
                DB = dbNumber,
                VarType = varType,
                StartByteAdr = startByte,
                BitAdr = (byte) (bitNumber == -1 ? 0 : bitNumber)
            };
        }

        /// <summary>
        /// Create an instance of <see cref="DataItem"/> from the supplied address and value.
        /// </summary>
        /// <param name="address">The address to create the DataItem for.</param>
        /// <param name="value">The value to be applied to the DataItem.</param>
        /// <returns>A new <see cref="DataItem"/> instance with properties parsed from <paramref name="address"/> and the supplied value set.</returns>
        public static DataItem FromAddressAndValue<T>(string address, T value)
        {
            var dataItem = FromAddress(address);
            dataItem.Value = value;

            if (typeof(T).IsArray)
            {
                var array = ((Array?)dataItem.Value);
                if ( array != null)
                {
                    dataItem.Count = array.Length;
                }
            }

            return dataItem;
        }
    }
}
