using System;

namespace S7.Net.Interfaces
{
    public interface IPlc : IDisposable
    {
        /// <summary>
        /// Ip address of the plc
        /// </summary>
        string IP { get; set; }

        /// <summary>
        /// Checks if the socket is connected and polls the other peer (the plc) to see if it's connected.
        /// This is the variable that you should continously check to see if the communication is working
        /// See also: http://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Cpu type of the plc
        /// </summary>
        CpuType CPU { get; set; }

        /// <summary>
        /// Rack of the plc
        /// </summary>
        Int16 Rack { get; set; }

        /// <summary>
        /// Slot of the CPU of the plc
        /// </summary>
        Int16 Slot { get; set; }

        /// <summary>
        /// Name of the plc (optional, is not used anywhere in the driver)
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Tag of the plc (optional, is not used anywhere in the driver)
        /// </summary>
        object Tag { get; set; }

        /// <summary>
        /// Pings the IP address and returns true if the result of the ping is Success.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Contains the last error registered when executing a function
        /// </summary>
		string LastErrorString { get; }

        /// <summary>
        /// Contains the last error code registered when executing a function
        /// </summary>
		ErrorCode LastErrorCode { get; }

        /// <summary>
        /// Open a socket and connects to the plc, sending all the corrected package and returning if the connection was successful (ErroreCode.NoError) of it was wrong.
        /// </summary>
        /// <returns>Returns ErrorCode.NoError if the connection was successful, otherwise check the ErrorCode</returns>
        ErrorCode Open();

        /// <summary>
        /// Disonnects from the plc and close the socket
        /// </summary>
        void Close();

        /// <summary>
        /// Reads up to 200 bytes from the plc and returns an array of bytes. You must specify the memory area type, memory are address, byte start address and bytes count.
        /// If the read was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="DB">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <param name="count">Byte count, if you want to read 120 bytes, set this to 120. This parameter can't be higher than 200. If you need more, use recursion.</param>
        /// <returns>Returns the bytes in an array</returns>
        byte[] ReadBytes(DataType dataType, int DB, int startByteAdr, int count);

        /// <summary>
        /// Read and decode a certain number of bytes of the "VarType" provided. 
        /// This can be used to read multiple consecutive variables of the same type (Word, DWord, Int, etc).
        /// If the read was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <param name="varType">Type of the variable/s that you are reading</param>
        /// <param name="varCount">Number of the variables (NOT number of bytes) to read</param>
        /// <returns></returns>
        object Read(DataType dataType, int db, int startByteAdr, VarType varType, int varCount);

        /// <summary>
        /// Reads a single variable from the plc, takes in input strings like "DB1.DBX0.0", "DB20.DBD200", "MB20", "T45", etc.
        /// If the read was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="variable">Input strings like "DB1.DBX0.0", "DB20.DBD200", "MB20", "T45", etc.</param>
        /// <returns>Returns an object that contains the value. This object must be cast accordingly.</returns>
        object Read(string variable);

        /// <summary>
        /// Reads all the bytes needed to fill a struct in C#, and return an object that can be casted to the struct.
        /// </summary>
        /// <param name="structType">Type of the struct to be readed (es.: TypeOf(MyStruct)).</param>
        /// <param name="db">Address of the DB.</param>
        /// <returns>Returns a struct that must be cast.</returns>
        object ReadStruct(Type structType, int db);

        /// <summary>
        /// Reads all the bytes needed to fill a struct in C#, starting from a certain address, and return an object that can be casted to the struct.
        /// </summary>
        /// <param name="structType">Type of the struct to be readed (es.: TypeOf(MyStruct)).</param>
        /// <param name="db">Address of the DB.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <returns>Returns a struct that must be cast.</returns>
        object ReadStruct(Type structType, int db, int startByteAdr);

        /// <summary>
        /// Reads all the bytes needed to fill a class in C#, and set all the properties values to the value that are read from the plc. 
        /// This reads ony properties, it doesn't read private variable or public variable without {get;set;} specified.
        /// </summary>
        /// <param name="sourceClass">Instance of the class that will store the values</param>       
        /// <param name="db">Index of the DB; es.: 1 is for DB1</param>
        void ReadClass(object sourceClass, int db);

        /// <summary>
        /// Reads all the bytes needed to fill a class in C#, starting from a certain address, and set all the properties values to the value that are read from the plc. 
        /// This reads ony properties, it doesn't read private variable or public variable without {get;set;} specified.
        /// </summary>
        /// <param name="sourceClass">Instance of the class that will store the values</param>       
        /// <param name="db">Index of the DB; es.: 1 is for DB1</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        void ReadClass(object sourceClass, int db, int startByteAdr);

        ErrorCode WriteBytes(DataType dataType, int db, int startByteAdr, byte[] value);
        object Write(DataType dataType, int db, int startByteAdr, object value);
        object Write(string variable, object value);
        ErrorCode WriteStruct(object structValue, int db);
        ErrorCode WriteClass(object classValue, int db);

        
    }
}