namespace S7.Net
{
    /// <summary>
    /// Types of S7 cpu supported by the library
    /// </summary>
    public enum CpuType
    {
        /// <summary>
        /// S7 200 cpu type
        /// </summary>
        S7200 = 0,

        /// <summary>
        /// Siemens Logo 0BA8
        /// </summary>
        Logo0BA8 = 1,

        /// <summary>
        /// S7 300 cpu type
        /// </summary>
        S7300 = 10,

        /// <summary>
        /// S7 400 cpu type
        /// </summary>
        S7400 = 20,

        /// <summary>
        /// S7 1200 cpu type
        /// </summary>
        S71200 = 30,

        /// <summary>
        /// S7 1500 cpu type
        /// </summary>
        S71500 = 40,
    }

    /// <summary>
    /// Types of error code that can be set after a function is called
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// The function has been executed correctly
        /// </summary>
        NoError = 0,

        /// <summary>
        /// Wrong type of CPU error
        /// </summary>
        WrongCPU_Type = 1,

        /// <summary>
        /// Connection error
        /// </summary>
        ConnectionError = 2,

        /// <summary>
        /// Ip address not available
        /// </summary>
        IPAddressNotAvailable,

        /// <summary>
        /// Wrong format of the variable
        /// </summary>
        WrongVarFormat = 10,

        /// <summary>
        /// Wrong number of received bytes
        /// </summary>
        WrongNumberReceivedBytes = 11,

        /// <summary>
        /// Error on send data
        /// </summary>
        SendData = 20,

        /// <summary>
        /// Error on read data
        /// </summary>
        ReadData = 30,

        /// <summary>
        /// Error on write data
        /// </summary>
        WriteData = 50
    }

    /// <summary>
    /// Types of memory area that can be read
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// Input area memory
        /// </summary>
        Input = 129,

        /// <summary>
        /// Output area memory
        /// </summary>
        Output = 130,

        /// <summary>
        /// Merkers area memory (M0, M0.0, ...)
        /// </summary>
        Memory = 131,

        /// <summary>
        /// DB area memory (DB1, DB2, ...)
        /// </summary>
        DataBlock = 132,

        /// <summary>
        /// Timer area memory(T1, T2, ...)
        /// </summary>
        Timer = 29,

        /// <summary>
        /// Counter area memory (C1, C2, ...)
        /// </summary>
        Counter = 28
    }

    /// <summary>
    /// Types
    /// </summary>
    public enum VarType
    {
        /// <summary>
        /// S7 Bit variable type (bool)
        /// </summary>
        Bit,

        /// <summary>
        /// S7 Byte variable type (8 bits)
        /// </summary>
        Byte,

        /// <summary>
        /// S7 Word variable type (16 bits, 2 bytes)
        /// </summary>
        Word,

        /// <summary>
        /// S7 DWord variable type (32 bits, 4 bytes)
        /// </summary>
        DWord,

        /// <summary>
        /// S7 Int variable type (16 bits, 2 bytes)
        /// </summary>
        Int,

        /// <summary>
        /// DInt variable type (32 bits, 4 bytes)
        /// </summary>
        DInt,

        /// <summary>
        /// Real variable type (32 bits, 4 bytes)
        /// </summary>
        Real,

        /// <summary>
        /// LReal variable type (64 bits, 8 bytes)
        /// </summary>
        LReal,

        /// <summary>
        /// Char Array / C-String variable type (variable)
        /// </summary>
        String,

        /// <summary>
        /// S7 String variable type (variable)
        /// </summary>
        S7String,

        /// <summary>
        /// Timer variable type
        /// </summary>
        Timer,

        /// <summary>
        /// Counter variable type
        /// </summary>
        Counter,

        /// <summary>
        /// DateTIme variable type
        /// </summary>
        DateTime,

        /// <summary>
        /// DateTimeLong variable type
        /// </summary>
        DateTimeLong
    }
}
