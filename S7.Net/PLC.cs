using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using S7.Net.Types;
using Double = System.Double;


namespace S7.Net
{
    /// <summary>
    /// Creates an instance of S7.Net driver
    /// </summary>
    public class Plc : IDisposable
    {
        private const int CONNECTION_TIMED_OUT_ERROR_CODE = 10060;

        private Socket _mSocket; //TCP connection to device

        /// <summary>
        /// Ip address of the plc
        /// </summary>
        public string IP { get; private set; }

        /// <summary>
        /// Cpu type of the plc
        /// </summary>
        public CpuType CPU { get; private set; }

        /// <summary>
        /// Rack of the plc
        /// </summary>
        public Int16 Rack { get; private set; }

        /// <summary>
        /// Slot of the CPU of the plc
        /// </summary>
        public Int16 Slot { get; private set; }
        
        /// <summary>
        /// Returns true if a connection to the plc can be established
        /// </summary>
        public bool IsAvailable
        {
            get
            {

#if NETFX_CORE
                return (!string.IsNullOrWhiteSpace(IP));
#else
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    return Connect(socket) == ErrorCode.NoError;
                }
#endif
            }
        }


        /// <summary>
        /// Checks if the socket is connected and polls the other peer (the plc) to see if it's connected.
        /// This is the variable that you should continously check to see if the communication is working
        /// See also: http://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
        /// </summary>
        public bool IsConnected
        {
            get
            {
                try
                {
                    if (_mSocket == null)
                        return false;
                    
#if NETFX_CORE
                    return _mSocket.Connected;
#else
                    return !((_mSocket.Poll(1000, SelectMode.SelectRead) && (_mSocket.Available == 0)) || !_mSocket.Connected);
#endif
                }
                catch { return false; }
            }
        }

        /// <summary>
        /// Contains the last error registered when executing a function
        /// </summary>
        public string LastErrorString { get; private set; }

        /// <summary>
        /// Contains the last error code registered when executing a function
        /// </summary>
        public ErrorCode LastErrorCode { get; private set; }
        
        /// <summary>
        /// Creates a PLC object with all the parameters needed for connections.
        /// For S7-1200 and S7-1500, the default is rack = 0 and slot = 0.
        /// You need slot > 0 if you are connecting to external ethernet card (CP).
        /// For S7-300 and S7-400 the default is rack = 0 and slot = 2.
        /// </summary>
        /// <param name="cpu">CpuType of the plc (select from the enum)</param>
        /// <param name="ip">Ip address of the plc</param>
        /// <param name="rack">rack of the plc, usually it's 0, but check in the hardware configuration of Step7 or TIA portal</param>
        /// <param name="slot">slot of the CPU of the plc, usually it's 2 for S7300-S7400, 0 for S7-1200 and S7-1500.
        ///  If you use an external ethernet card, this must be set accordingly.</param>
        public Plc(CpuType cpu, string ip, Int16 rack, Int16 slot)
        {
            IP = ip;
            CPU = cpu;
            Rack = rack;
            Slot = slot;
        }

        private ErrorCode Connect(Socket socket)
        {
            try
            {
                IPEndPoint server = new IPEndPoint(IPAddress.Parse(IP), 102);
                socket.Connect(server);

                return ErrorCode.NoError;
            }
            catch (SocketException sex)
            {
                // see https://msdn.microsoft.com/en-us/library/windows/desktop/ms740668(v=vs.85).aspx
                if (sex.SocketErrorCode == SocketError.TimedOut)
                {
                    LastErrorCode = ErrorCode.IPAddressNotAvailable;
                }
                else
                {
                    LastErrorCode = ErrorCode.ConnectionError;
                }

                LastErrorString = sex.Message;
            }
            catch (Exception ex)
            {
                LastErrorCode = ErrorCode.ConnectionError;
                LastErrorString = ex.Message;
            }

            return LastErrorCode;
        }

        /// <summary>
        /// Open a socket and connects to the plc, sending all the corrected package and returning if the connection was successful (ErroreCode.NoError) of it was wrong.
        /// </summary>
        /// <returns>Returns ErrorCode.NoError if the connection was successful, otherwise check the ErrorCode</returns>
        public ErrorCode Open()
        {
            byte[] bReceive = new byte[256];

            _mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
            _mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);

            if (Connect(_mSocket) != ErrorCode.NoError)
            {
                return LastErrorCode;
            }

            try 
            {
                byte[] bSend1 = { 3, 0, 0, 22, 17, 224, 0, 0, 0, 46, 0, 193, 2, 1, 0, 194, 2, 3, 0, 192, 1, 9 };

                switch (CPU) {
                    case CpuType.S7200:
                        //S7200: Chr(193) & Chr(2) & Chr(16) & Chr(0) 'Eigener Tsap
                        bSend1[11] = 193;
                        bSend1[12] = 2;
                        bSend1[13] = 16;
                        bSend1[14] = 0;
                        //S7200: Chr(194) & Chr(2) & Chr(16) & Chr(0) 'Fremder Tsap
                        bSend1[15] = 194;
                        bSend1[16] = 2;
                        bSend1[17] = 16;
                        bSend1[18] = 0;
                        break;
                    case CpuType.S71200:
                    case CpuType.S7300:
                        //S7300: Chr(193) & Chr(2) & Chr(1) & Chr(0)  'Eigener Tsap
                        bSend1[11] = 193;
                        bSend1[12] = 2;
                        bSend1[13] = 1;
                        bSend1[14] = 0;
                        //S7300: Chr(194) & Chr(2) & Chr(3) & Chr(2)  'Fremder Tsap
                        bSend1[15] = 194;
                        bSend1[16] = 2;
                        bSend1[17] = 3;
                        bSend1[18] = (byte)(Rack * 2 * 16 + Slot);
                        break;
                    case CpuType.S7400:
                        //S7400: Chr(193) & Chr(2) & Chr(1) & Chr(0)  'Eigener Tsap
                        bSend1[11] = 193;
                        bSend1[12] = 2;
                        bSend1[13] = 1;
                        bSend1[14] = 0;
                        //S7400: Chr(194) & Chr(2) & Chr(3) & Chr(3)  'Fremder Tsap
                        bSend1[15] = 194;
                        bSend1[16] = 2;
                        bSend1[17] = 3;
                        bSend1[18] = (byte)(Rack * 2 * 16 + Slot);
                        break;
                    case CpuType.S71500:
                        // Eigener Tsap
                        bSend1[11] = 193;
                        bSend1[12] = 2;
                        bSend1[13] = 0x10;
                        bSend1[14] = 0x2;
                        // Fredmer Tsap
                        bSend1[15] = 194;
                        bSend1[16] = 2;
                        bSend1[17] = 0x3;
                        bSend1[18] = (byte)(Rack * 2 * 16 + Slot);
                        break;
                    default:
                        return ErrorCode.WrongCPU_Type;
                }

                _mSocket.Send(bSend1, 22, SocketFlags.None);
                if (_mSocket.Receive(bReceive, 22, SocketFlags.None) != 22)
                {
                    throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());
                } 

                byte[] bsend2 = { 3, 0, 0, 25, 2, 240, 128, 50, 1, 0, 0, 255, 255, 0, 8, 0, 0, 240, 0, 0, 3, 0, 3, 1, 0 };
                _mSocket.Send(bsend2, 25, SocketFlags.None);

                if (_mSocket.Receive(bReceive, 27, SocketFlags.None) != 27)
                {
                    throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());
                } 
            }
            catch(Exception exc)
            {
                LastErrorCode = ErrorCode.ConnectionError;
                LastErrorString = "Couldn't establish the connection to " + IP + ".\nMessage: " + exc.Message;
                return ErrorCode.ConnectionError;
            }

            return ErrorCode.NoError;
        }

        /// <summary>
        /// Disonnects from the plc and close the socket
        /// </summary>
        public void Close()
        {
            if (_mSocket != null && _mSocket.Connected) 
            {
                _mSocket.Shutdown(SocketShutdown.Both);
                _mSocket.Close();
            }
        }

        /// <summary>
        /// Reads multiple vars in a single request. 
        /// You have to create and pass a list of DataItems and you obtain in response the same list with the values.
        /// Values are stored in the property "Value" of the dataItem and are already converted.
        /// If you don't want the conversion, just create a dataItem of bytes. 
        /// DataItems must not be more than 20 (protocol restriction) and bytes must not be more than 200 + 22 of header (protocol restriction).
        /// </summary>
        /// <param name="dataItems">List of dataitems that contains the list of variables that must be read. Maximum 20 dataitems are accepted.</param>
        public void ReadMultipleVars(List<DataItem> dataItems)
        {
            int cntBytes = dataItems.Sum(dataItem => VarTypeToByteLength(dataItem.VarType, dataItem.Count));
            
            if (dataItems.Count > 20) throw new Exception("Too many vars requested");
            if (cntBytes > 222) throw new Exception("Too many bytes requested"); //todo, proper TDU check + split in multiple requests

            try
            {
                // first create the header
                int packageSize = 19 + (dataItems.Count*12);
                Types.ByteArray package = new ByteArray(packageSize);
                package.Add(ReadHeaderPackage(dataItems.Count));
                // package.Add(0x02);  // datenart
                foreach (var dataItem in dataItems)
                {
                    package.Add(CreateReadDataRequestPackage(dataItem.DataType, dataItem.DB, dataItem.StartByteAdr, VarTypeToByteLength(dataItem.VarType, dataItem.Count)));
                }

                _mSocket.Send(package.array, package.array.Length, SocketFlags.None);

                byte[] bReceive = new byte[512];
                int numReceived = _mSocket.Receive(bReceive, 512, SocketFlags.None);
                if (bReceive[21] != 0xff) throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());

                int offset = 25;
                foreach (var dataItem in dataItems)
                {
                    int byteCnt = VarTypeToByteLength(dataItem.VarType, dataItem.Count);
                    byte[] bytes = new byte[byteCnt];

                    for (int i = 0; i < byteCnt; i++)
                    {
                        bytes[i] = bReceive[i + offset];
                    }

                    offset += byteCnt + 4;

                    dataItem.Value = ParseBytes(dataItem.VarType, bytes, dataItem.Count);
                }
            }
            catch (SocketException socketException)
            {
                LastErrorCode = ErrorCode.WriteData;
                LastErrorString = socketException.Message;
            }
            catch (Exception exc)
            {
                LastErrorCode = ErrorCode.WriteData;
                LastErrorString = exc.Message;
            }
        }
        
        /// <summary>
        /// Reads a number of bytes from a DB starting from a specified index. This handles more than 200 bytes with multiple requests.
        /// If the read was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <param name="count">Byte count, if you want to read 120 bytes, set this to 120.</param>
        /// <returns>Returns the bytes in an array</returns>
        public byte[] ReadBytes(DataType dataType, int db, int startByteAdr, int count)
        {
            List<byte> resultBytes = new List<byte>();
            int index = startByteAdr;
            while (count > 0)
            {
                var maxToRead = (int)Math.Min(count, 200);
                byte[] bytes = ReadBytesWithASingleRequest(dataType, db, index, maxToRead);
                if (bytes == null)
                    return resultBytes.ToArray();
                resultBytes.AddRange(bytes);
                count -= maxToRead;
                index += maxToRead;
            }
            return resultBytes.ToArray();
        }

        /// <summary>
        /// Read and decode a certain number of bytes of the "VarType" provided. 
        /// This can be used to read multiple consecutive variables of the same type (Word, DWord, Int, etc).
        /// If the read was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <param name="varType">Type of the variable/s that you are reading</param>
        /// <param name="bitAdr">Address of bit. If you want to read DB1.DBX200.6, set 6 to this parameter.</param>
        /// <param name="varCount"></param>
        public object Read(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0)
        {
            int cntBytes = VarTypeToByteLength(varType, varCount);
            byte[] bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);

            return ParseBytes(varType, bytes, varCount, bitAdr);
        }
        
        /// <summary>
        /// Reads a single variable from the plc, takes in input strings like "DB1.DBX0.0", "DB20.DBD200", "MB20", "T45", etc.
        /// If the read was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="variable">Input strings like "DB1.DBX0.0", "DB20.DBD200", "MB20", "T45", etc.</param>
        /// <returns>Returns an object that contains the value. This object must be cast accordingly.</returns>
        public object Read(string variable)
        {
            DataType mDataType;
            int mDB;
            int mByte;
            int mBit;

            byte objByte;
            UInt16 objUInt16;
            UInt32 objUInt32;
            double objDouble;
            BitArray objBoolArray;

            string txt = variable.ToUpper();
            txt = txt.Replace(" ", "");     // remove spaces

            try
            {
                switch (txt.Substring(0, 2))
                {
                    case "DB":
                        string[] strings = txt.Split(new char[] { '.' });
                        if (strings.Length < 2)
                            throw new Exception();

                        mDB = int.Parse(strings[0].Substring(2));
                        string dbType = strings[1].Substring(0, 3);
                        int dbIndex = int.Parse(strings[1].Substring(3));

                        switch (dbType)
                        {
                            case "DBB":
                                byte obj = (byte)Read(DataType.DataBlock, mDB, dbIndex, VarType.Byte, 1);
                                return obj;
                            case "DBW":
                                UInt16 objI = (UInt16)Read(DataType.DataBlock, mDB, dbIndex, VarType.Word, 1);
                                return objI;
                            case "DBD":
                                UInt32 objU = (UInt32)Read(DataType.DataBlock, mDB, dbIndex, VarType.DWord, 1);
                                return objU;
                            case "DBX":
                                mByte = dbIndex;
                                mBit = int.Parse(strings[2]);
                                if (mBit > 7) throw new Exception();
                                byte obj2 = (byte)Read(DataType.DataBlock, mDB, mByte, VarType.Byte, 1);
                                objBoolArray = new BitArray(new byte[] { obj2 });
                                return objBoolArray[mBit];
                            default:
                                throw new Exception();
                        }
                    case "EB":
                        // Input byte
                        objByte = (byte)Read(DataType.Input, 0, int.Parse(txt.Substring(2)), VarType.Byte, 1);
                        return objByte;
                    case "EW":
                        // Input word
                        objUInt16 = (UInt16)Read(DataType.Input, 0, int.Parse(txt.Substring(2)), VarType.Word, 1);
                        return objUInt16;
                    case "ED":
                        // Input double-word
                        objUInt32 = (UInt32)Read(DataType.Input, 0, int.Parse(txt.Substring(2)), VarType.DWord, 1);
                        return objUInt32;
                    case "AB":
                        // Output byte
                        objByte = (byte)Read(DataType.Output, 0, int.Parse(txt.Substring(2)), VarType.Byte, 1);
                        return objByte;
                    case "AW":
                        // Output word
                        objUInt16 = (UInt16)Read(DataType.Output, 0, int.Parse(txt.Substring(2)), VarType.Word, 1);
                        return objUInt16;
                    case "AD":
                        // Output double-word
                        objUInt32 = (UInt32)Read(DataType.Output, 0, int.Parse(txt.Substring(2)), VarType.DWord, 1);
                        return objUInt32;
                    case "MB":
                        // Memory byte
                        objByte = (byte)Read(DataType.Memory, 0, int.Parse(txt.Substring(2)), VarType.Byte, 1);
                        return objByte;
                    case "MW":
                        // Memory word
                        objUInt16 = (UInt16)Read(DataType.Memory, 0, int.Parse(txt.Substring(2)), VarType.Word, 1);
                        return objUInt16;
                    case "MD":
                        // Memory double-word
                        objUInt32 = (UInt32)Read(DataType.Memory, 0, int.Parse(txt.Substring(2)), VarType.DWord, 1);
                        return objUInt32;
                    default:
                        switch (txt.Substring(0, 1))
                        {
                            case "E":
                            case "I":
                                // Input
                                mDataType = DataType.Input;
                                break;
                            case "A":
                            case "O":
                                // Output
                                mDataType = DataType.Output;
                                break;
                            case "M":
                                // Memory
                                mDataType = DataType.Memory;
                                break;
                            case "T":
                                // Timer
                                objDouble = (double)Read(DataType.Timer, 0, int.Parse(txt.Substring(1)), VarType.Timer, 1);
                                return objDouble;
                            case "Z":
                            case "C":
                                // Counter
                                objUInt16 = (UInt16)Read(DataType.Counter, 0, int.Parse(txt.Substring(1)), VarType.Counter, 1);
                                return objUInt16;
                            default:
                                throw new Exception();
                        }

                        string txt2 = txt.Substring(1);
                        if (txt2.IndexOf(".") == -1) throw new Exception();

                        mByte = int.Parse(txt2.Substring(0, txt2.IndexOf(".")));
                        mBit = int.Parse(txt2.Substring(txt2.IndexOf(".") + 1));
                        if (mBit > 7) throw new Exception();
                        var obj3 = (byte)Read(mDataType, 0, mByte, VarType.Byte, 1);
                        objBoolArray = new BitArray(new byte[]{obj3});
                        return objBoolArray[mBit];
                }
            }
            catch 
            {
                LastErrorCode = ErrorCode.WrongVarFormat;
                LastErrorString = "The variable'" + variable + "' could not be read. Please check the syntax and try again.";
                return LastErrorCode;
            }
        }

        /// <summary>
        /// Reads all the bytes needed to fill a struct in C#, starting from a certain address, and return an object that can be casted to the struct.
        /// </summary>
        /// <param name="structType">Type of the struct to be readed (es.: TypeOf(MyStruct)).</param>
        /// <param name="db">Address of the DB.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <returns>Returns a struct that must be cast.</returns>
        public object ReadStruct(Type structType, int db, int startByteAdr = 0)
        {
            int numBytes = Types.Struct.GetStructSize(structType);
            // now read the package
            var resultBytes = ReadBytes(DataType.DataBlock, db, startByteAdr, numBytes);

            // and decode it
            return Types.Struct.FromBytes(structType, resultBytes);
        }

        /// <summary>
        /// Reads all the bytes needed to fill a struct in C#, starting from a certain address, and returns the struct or null if nothing was read.
        /// </summary>
        /// <typeparam name="T">The struct type</typeparam>
        /// <param name="db">Address of the DB.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <returns>Returns a nulable struct. If nothing was read null will be returned.</returns>
        public T? ReadStruct<T>(int db, int startByteAdr = 0) where T : struct
        {
            return ReadStruct(typeof(T), db, startByteAdr) as T?;
        }

        /// <summary>
        /// Reads all the bytes needed to fill a class in C#, starting from a certain address, and set all the properties values to the value that are read from the plc. 
        /// This reads only properties, it doesn't read private variable or public variable without {get;set;} specified.
        /// </summary>
        /// <param name="sourceClass">Instance of the class that will store the values</param>       
        /// <param name="db">Index of the DB; es.: 1 is for DB1</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <returns>The number of read bytes</returns>
        public int ReadClass(object sourceClass, int db, int startByteAdr = 0)
        {
            int numBytes = Types.Class.GetClassSize(sourceClass);
			if(numBytes <= 0)
            {
                throw new Exception("The size of the class is less than 1 byte and therefore cannot be read");
            }
			
            // now read the package
            var resultBytes = ReadBytes(DataType.DataBlock, db, startByteAdr, numBytes);
            // and decode it
            Types.Class.FromBytes(sourceClass, resultBytes);

            return resultBytes.Length;
        }

        /// <summary>
        /// Reads all the bytes needed to fill a class in C#, starting from a certain address, and set all the properties values to the value that are read from the plc. 
        /// This reads only properties, it doesn't read private variable or public variable without {get;set;} specified. To instantiate the class defined by the generic
        /// type, the class needs a default constructor.
        /// </summary>
        /// <typeparam name="T">The class that will be instantiated. Requires a default constructor</typeparam>
        /// <param name="db">Index of the DB; es.: 1 is for DB1</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <returns>An instance of the class with the values read from the plc. If no data has been read, null will be returned</returns>
        public T ReadClass<T>(int db, int startByteAdr = 0) where T : class
        {
            return ReadClass(() => Activator.CreateInstance<T>(), db, startByteAdr);
        }

        /// <summary>
        /// Reads all the bytes needed to fill a class in C#, starting from a certain address, and set all the properties values to the value that are read from the plc. 
        /// This reads only properties, it doesn't read private variable or public variable without {get;set;} specified.
        /// </summary>
        /// <typeparam name="T">The class that will be instantiated</typeparam>
        /// <param name="classFactory">Function to instantiate the class</param>
        /// <param name="db">Index of the DB; es.: 1 is for DB1</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <returns>An instance of the class with the values read from the plc. If no data has been read, null will be returned</returns>
        public T ReadClass<T>(Func<T> classFactory, int db, int startByteAdr = 0) where T : class
        {
            var instance = classFactory();
            int readBytes = ReadClass(instance, db, startByteAdr);
            if (readBytes <= 0)
            {
                return null;
            }

            return instance;
        }


        /// <summary>
        /// Write a number of bytes from a DB starting from a specified index. This handles more than 200 bytes with multiple requests.
        /// If the write was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to write DB1.DBW200, this is 200.</param>
        /// <param name="value">Bytes to write. If more than 200, multiple requests will be made.</param>
        /// <returns>NoError if it was successful, or the error is specified</returns>
        public ErrorCode WriteBytes(DataType dataType, int db, int startByteAdr, byte[] value)
        {
            int localIndex = 0;
            int count = value.Length;
            while (count > 0)
            {
                var maxToWrite = (int)Math.Min(count, 200);
                ErrorCode lastError = WriteBytesWithASingleRequest(dataType, db, startByteAdr + localIndex, value.Skip(localIndex).Take(maxToWrite).ToArray());
                if (lastError != ErrorCode.NoError)
                {
                    return lastError;
                }
                count -= maxToWrite;
                localIndex += maxToWrite;
            }
            return ErrorCode.NoError;
        }

        /// <summary>
        /// Write a single bit from a DB with the specified index.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to write DB1.DBW200, this is 200.</param>
        /// <param name="bitAdr">The address of the bit. (0-7)</param>
        /// <param name="value">Bytes to write. If more than 200, multiple requests will be made.</param>
        /// <returns>NoError if it was successful, or the error is specified</returns>
        public ErrorCode WriteBit(DataType dataType, int db, int startByteAdr, int bitAdr, bool value)
        {
            if (bitAdr < 0 || bitAdr > 7)
                throw new Exception(string.Format("Addressing Error: You can only reference bitwise locations 0-7. Address {0} is invalid", bitAdr));

            ErrorCode lastError = WriteBitWithASingleRequest(dataType, db, startByteAdr, bitAdr, value);
            if (lastError != ErrorCode.NoError)
            {
                return lastError;
            }

            return ErrorCode.NoError;
        }

        /// <summary>
        /// Write a single bit from a DB with the specified index.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to write DB1.DBW200, this is 200.</param>
        /// <param name="bitAdr">The address of the bit. (0-7)</param>
        /// <param name="value">Bytes to write. If more than 200, multiple requests will be made.</param>
        /// <returns>NoError if it was successful, or the error is specified</returns>
        public ErrorCode WriteBit(DataType dataType, int db, int startByteAdr, int bitAdr, int value)
        {
            if (value < 0 || value > 1)
                throw new ArgumentException("Value must be 0 or 1", nameof(value));

            return WriteBit(dataType, db, startByteAdr, bitAdr, value == 1);
        }

        /// <summary>
        /// Takes in input an object and tries to parse it to an array of values. This can be used to write many data, all of the same type.
        /// You must specify the memory area type, memory are address, byte start address and bytes count.
        /// If the read was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <param name="value">Bytes to write. The lenght of this parameter can't be higher than 200. If you need more, use recursion.</param>
        /// <param name="bitAdr">The address of the bit. (0-7)</param>
        /// <returns>NoError if it was successful, or the error is specified</returns>
        public ErrorCode Write(DataType dataType, int db, int startByteAdr, object value, int bitAdr = -1)
        {
            byte[] package = null;

            if (bitAdr != -1)
            {
                //Must be writing a bit value as bitAdr is specified
                bool bitValue = false;
                if (value is bool)
                {
                    bitValue = (bool) value;
                }
                else if (value is int)
                {
                    var intValue = (int) value;
                    if (intValue < 0 || intValue > 7)
                        throw new Exception(string.Format("Addressing Error: You can only reference bitwise locations 0-7. Address {0} is invalid", bitAdr));

                    bitValue = intValue == 1;
                }
                else
                {
                    throw new ArgumentException("Value must be a bool or an int to write a bit", nameof(value));
                }

                return WriteBit(dataType, db, startByteAdr, bitAdr, bitValue);
            }

            switch (value.GetType().Name)
            {
                case "Byte":
                    package = Types.Byte.ToByteArray((byte)value);
                    break;
                case "Int16":
                    package = Types.Int.ToByteArray((Int16)value);
                    break;
                case "UInt16":
                    package = Types.Word.ToByteArray((UInt16)value);
                    break;
                case "Int32":
                    package = Types.DInt.ToByteArray((Int32)value);
                    break;
                case "UInt32":
                    package = Types.DWord.ToByteArray((UInt32)value);
                    break;
                case "Double":
                    package = Types.Double.ToByteArray((Double)value);
                    break;
                case "Byte[]":
                    package = (byte[])value;
                    break;
                case "Int16[]":
                    package = Types.Int.ToByteArray((Int16[])value);
                    break;
                case "UInt16[]":
                    package = Types.Word.ToByteArray((UInt16[])value);
                    break;
                case "Int32[]":
                    package = Types.DInt.ToByteArray((Int32[])value);
                    break;
                case "UInt32[]":
                    package = Types.DWord.ToByteArray((UInt32[])value);
                    break;
                case "Double[]":
                    package = Types.Double.ToByteArray((double[])value);
                    break;
                case "String":
                    package = Types.String.ToByteArray(value as string);
                    break;
                default:
                    return ErrorCode.WrongVarFormat;
            }

            return WriteBytes(dataType, db, startByteAdr, package);
        }

        /// <summary>
        /// Writes a single variable from the plc, takes in input strings like "DB1.DBX0.0", "DB20.DBD200", "MB20", "T45", etc.
        /// If the write was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="variable">Input strings like "DB1.DBX0.0", "DB20.DBD200", "MB20", "T45", etc.</param>
        /// <param name="value">Value to be written to the plc</param>
        /// <returns>NoError if it was successful, or the error is specified</returns>
        public ErrorCode Write(string variable, object value)
        {
            DataType mDataType;
            int mDB;
            int mByte;
            int mBit;

            string addressLocation;
            byte _byte;
            object objValue;

            string txt = variable.ToUpper();
            txt = txt.Replace(" ", ""); // Remove spaces

            try
            {
                switch (txt.Substring(0, 2))
                {
                    case "DB":
                        string[] strings = txt.Split(new char[]{'.'});
                        if (strings.Length < 2)
                            throw new Exception();

                        mDB = int.Parse(strings[0].Substring(2));
                        string dbType = strings[1].Substring(0, 3);
                        int dbIndex = int.Parse(strings[1].Substring(3));                       
                       
                        switch (dbType)
                        {
                            case "DBB":
                                objValue = Convert.ChangeType(value, typeof(byte));
                                return Write(DataType.DataBlock, mDB, dbIndex, (byte)objValue);
                            case "DBW":
                                if (value is short)
                                {
                                    objValue = ((short)value).ConvertToUshort();
                                }
                                else
                                {
                                    objValue = Convert.ChangeType(value, typeof(UInt16));
                                }
                                return Write(DataType.DataBlock, mDB, dbIndex, (UInt16)objValue);
                            case "DBD":
                                if (value is int)
                                {
                                    return Write(DataType.DataBlock, mDB, dbIndex, (Int32)value);
                                }
                                else
                                {
                                    objValue = Convert.ChangeType(value, typeof(UInt32));
                                }
                                return Write(DataType.DataBlock, mDB, dbIndex, (UInt32)objValue);
                            case "DBX":
                                mByte = dbIndex;
                                mBit = int.Parse(strings[2]);
                                if (mBit > 7)
                                {
                                    throw new Exception(string.Format("Addressing Error: You can only reference bitwise locations 0-7. Address {0} is invalid", mBit));
                                }

                                return Write(DataType.DataBlock, mDB, mByte, value, mBit);
                            case "DBS":
                                // DB-String
                                return Write(DataType.DataBlock, mDB, dbIndex, (string)value);
                            default:
                                throw new Exception(string.Format("Addressing Error: Unable to parse address {0}. Supported formats include DBB (byte), DBW (word), DBD (dword), DBX (bitwise), DBS (string).", dbType));
                        }
                    case "EB":
                        // Input Byte
                        objValue = Convert.ChangeType(value, typeof(byte));
                        return Write(DataType.Input, 0, int.Parse(txt.Substring(2)), (byte)objValue);
                    case "EW":
                        // Input Word
                        objValue = Convert.ChangeType(value, typeof(UInt16));
                        return Write(DataType.Input, 0, int.Parse(txt.Substring(2)), (UInt16)objValue);
                    case "ED":
                        // Input Double-Word
                        objValue = Convert.ChangeType(value, typeof(UInt32));
                        return Write(DataType.Input, 0, int.Parse(txt.Substring(2)), (UInt32)objValue);
                    case "AB":
                        // Output Byte
                        objValue = Convert.ChangeType(value, typeof(byte));
                        return Write(DataType.Output, 0, int.Parse(txt.Substring(2)), (byte)objValue);
                    case "AW":
                        // Output Word
                        objValue = Convert.ChangeType(value, typeof(UInt16));
                        return Write(DataType.Output, 0, int.Parse(txt.Substring(2)), (UInt16)objValue);
                    case "AD":
                        // Output Double-Word
                        objValue = Convert.ChangeType(value, typeof(UInt32));
                        return Write(DataType.Output, 0, int.Parse(txt.Substring(2)), (UInt32)objValue);
                    case "MB":
                        // Memory Byte
                        objValue = Convert.ChangeType(value, typeof(byte));
                        return Write(DataType.Memory, 0, int.Parse(txt.Substring(2)), (byte)objValue);
                    case "MW":
                        // Memory Word
                        objValue = Convert.ChangeType(value, typeof(UInt16));
                        return Write(DataType.Memory, 0, int.Parse(txt.Substring(2)), (UInt16)objValue);
                    case "MD":
                        // Memory Double-Word
                        return Write(DataType.Memory, 0, int.Parse(txt.Substring(2)), value);
                    default:
                        switch (txt.Substring(0, 1))
                        {
                            case "E":
                            case "I":
                                // Input
                                mDataType = DataType.Input;
                                break;
                            case "A":
                            case "O":
                                // Output
                                mDataType = DataType.Output;
                                break;
                            case "M":
                                // Memory
                                mDataType = DataType.Memory;
                                break;
                            case "T":
                                // Timer
                                return Write(DataType.Timer, 0, int.Parse(txt.Substring(1)), (double)value);
                            case "Z":
                            case "C":
                                // Counter
                                return Write(DataType.Counter, 0, int.Parse(txt.Substring(1)), (short)value);
                            default:
                                throw new Exception(string.Format("Unknown variable type {0}.",txt.Substring(0,1)));
                        }

                        addressLocation = txt.Substring(1);
                        int decimalPointIndex = addressLocation.IndexOf(".");
                        if (decimalPointIndex == -1)
                        {
                            throw new Exception(string.Format("Cannot parse variable {0}. Input, Output, Memory Address, Timer, and Counter types require bit-level addressing (e.g. I0.1).",addressLocation));
                        }

                        mByte = int.Parse(addressLocation.Substring(0, decimalPointIndex));
                        mBit = int.Parse(addressLocation.Substring(decimalPointIndex + 1));
                        if (mBit > 7)
                        {
                            throw new Exception(string.Format("Addressing Error: You can only reference bitwise locations 0-7. Address {0} is invalid", mBit));
                        }

                        _byte = (byte)Read(mDataType, 0, mByte, VarType.Byte, 1);
                        if ((int)value == 1)
                            _byte = (byte)(_byte | (byte)Math.Pow(2, mBit));      // Set bit
                        else
                            _byte = (byte)(_byte & (_byte ^ (byte)Math.Pow(2, mBit))); // Reset bit

                        return Write(mDataType, 0, mByte, (byte)_byte);
                }
            }
            catch(Exception exc)
            {
                LastErrorCode = ErrorCode.WrongVarFormat;
                LastErrorString = "The variable'" + variable + "' could not be parsed. Please check the syntax and try again.\nException: " + exc.Message;
                return LastErrorCode;
            }
        }

        /// <summary>
        /// Writes a C# struct to a DB in the plc
        /// </summary>
        /// <param name="structValue">The struct to be written</param>
        /// <param name="db">Db address</param>
        /// <param name="startByteAdr">Start bytes on the plc</param>
        /// <returns>NoError if it was successful, or the error is specified</returns>
        public ErrorCode WriteStruct(object structValue, int db, int startByteAdr = 0)
        {
            var bytes = Types.Struct.ToBytes(structValue).ToList();
            var errCode = WriteBytes(DataType.DataBlock ,db, startByteAdr, bytes.ToArray());
            return errCode;
        }

        /// <summary>
        /// Writes a C# class to a DB in the plc
        /// </summary>
        /// <param name="classValue">The class to be written</param>
        /// <param name="db">Db address</param>
        /// <param name="startByteAdr">Start bytes on the plc</param>
        /// <returns>NoError if it was successful, or the error is specified</returns>
        public ErrorCode WriteClass(object classValue, int db, int startByteAdr = 0)
        {
            var bytes = Types.Class.ToBytes(classValue).ToList();
            var errCode = WriteBytes(DataType.DataBlock, db, startByteAdr, bytes.ToArray());
            return errCode;
        }

        /// <summary>
        /// Sets the LastErrorCode to NoError and LastErrorString to String.Empty
        /// </summary>
        public void ClearLastError()
        {
            LastErrorCode = ErrorCode.NoError;
            LastErrorString = string.Empty;
        }

        /// <summary>
        /// Creates the header to read bytes from the plc
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        private Types.ByteArray ReadHeaderPackage(int amount = 1)
        {
            //header size = 19 bytes
            var package = new Types.ByteArray(19);
            package.Add(new byte[] { 0x03, 0x00, 0x00 });
            //complete package size
            package.Add((byte)(19 + (12 * amount)));
            package.Add(new byte[] { 0x02, 0xf0, 0x80, 0x32, 0x01, 0x00, 0x00, 0x00, 0x00 });
            //data part size
            package.Add(Types.Word.ToByteArray((ushort)(2 + (amount * 12))));
            package.Add(new byte[] { 0x00, 0x00, 0x04 });
            //amount of requests
            package.Add((byte)amount);

            return package;
        }

        /// <summary>
        /// Create the bytes-package to request data from the plc. You have to specify the memory type (dataType), 
        /// the address of the memory, the address of the byte and the bytes count. 
        /// </summary>
        /// <param name="dataType">MemoryType (DB, Timer, Counter, etc.)</param>
        /// <param name="db">Address of the memory to be read</param>
        /// <param name="startByteAdr">Start address of the byte</param>
        /// <param name="count">Number of bytes to be read</param>
        /// <returns></returns>
        private Types.ByteArray CreateReadDataRequestPackage(DataType dataType, int db, int startByteAdr, int count = 1)
        {
            //single data req = 12
            var package = new Types.ByteArray(12);
            package.Add(new byte[] { 0x12, 0x0a, 0x10 });
            switch (dataType)
            {
                case DataType.Timer:
                case DataType.Counter:
                    package.Add((byte)dataType);
                    break;
                default:
                    package.Add(0x02);
                    break;
            }

            package.Add(Types.Word.ToByteArray((ushort)(count)));
            package.Add(Types.Word.ToByteArray((ushort)(db)));
            package.Add((byte)dataType);
            var overflow = (int)(startByteAdr * 8 / 0xffffU); // handles words with address bigger than 8191
            package.Add((byte)overflow);
            switch (dataType)
            {
                case DataType.Timer:
                case DataType.Counter:
                    package.Add(Types.Word.ToByteArray((ushort)(startByteAdr)));
                    break;
                default:
                    package.Add(Types.Word.ToByteArray((ushort)((startByteAdr) * 8)));
                    break;
            }

            return package;
        }

        private byte[] ReadBytesWithASingleRequest(DataType dataType, int db, int startByteAdr, int count)
        {
            byte[] bytes = new byte[count];

            try
            {
                // first create the header
                int packageSize = 31;
                Types.ByteArray package = new ByteArray(packageSize);
                package.Add(ReadHeaderPackage());
                // package.Add(0x02);  // datenart
                package.Add(CreateReadDataRequestPackage(dataType, db, startByteAdr, count));

                _mSocket.Send(package.array, package.array.Length, SocketFlags.None);

                byte[] bReceive = new byte[512];
                int numReceived = _mSocket.Receive(bReceive, 512, SocketFlags.None);
                if (bReceive[21] != 0xff)
                    throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());

                for (int cnt = 0; cnt < count; cnt++)
                    bytes[cnt] = bReceive[cnt + 25];

                return bytes;
            }
            catch (SocketException socketException)
            {
                LastErrorCode = ErrorCode.WriteData;
                LastErrorString = socketException.Message;
                return null;
            }
            catch (Exception exc)
            {
                LastErrorCode = ErrorCode.WriteData;
                LastErrorString = exc.Message;
                return null;
            }
        }

        /// <summary>
        /// Writes up to 200 bytes to the plc and returns NoError if successful. You must specify the memory area type, memory are address, byte start address and bytes count.
        /// If the write was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <param name="value">Bytes to write. The lenght of this parameter can't be higher than 200. If you need more, use recursion.</param>
        /// <returns>NoError if it was successful, or the error is specified</returns>
        private ErrorCode WriteBytesWithASingleRequest(DataType dataType, int db, int startByteAdr, byte[] value)
        {
            byte[] bReceive = new byte[513];
            int varCount = 0;

            try
            {
                varCount = value.Length;
                // first create the header
                int packageSize = 35 + value.Length;
                Types.ByteArray package = new Types.ByteArray(packageSize);

                package.Add(new byte[] { 3, 0, 0 });
                package.Add((byte)packageSize);
                package.Add(new byte[] { 2, 0xf0, 0x80, 0x32, 1, 0, 0 });
                package.Add(Types.Word.ToByteArray((ushort)(varCount - 1)));
                package.Add(new byte[] { 0, 0x0e });
                package.Add(Types.Word.ToByteArray((ushort)(varCount + 4)));
                package.Add(new byte[] { 0x05, 0x01, 0x12, 0x0a, 0x10, 0x02 });
                package.Add(Types.Word.ToByteArray((ushort)varCount));
                package.Add(Types.Word.ToByteArray((ushort)(db)));
                package.Add((byte)dataType);
                var overflow = (int)(startByteAdr * 8 / 0xffffU); // handles words with address bigger than 8191
                package.Add((byte)overflow);
                package.Add(Types.Word.ToByteArray((ushort)(startByteAdr * 8)));
                package.Add(new byte[] { 0, 4 });
                package.Add(Types.Word.ToByteArray((ushort)(varCount * 8)));

                // now join the header and the data
                package.Add(value);

                _mSocket.Send(package.array, package.array.Length, SocketFlags.None);

                int numReceived = _mSocket.Receive(bReceive, 512, SocketFlags.None);
                if (bReceive[21] != 0xff)
                {
                    throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());
                }

                return ErrorCode.NoError;
            }
            catch (Exception exc)
            {
                LastErrorCode = ErrorCode.WriteData;
                LastErrorString = exc.Message;
                return LastErrorCode;
            }
        }

        private ErrorCode WriteBitWithASingleRequest(DataType dataType, int db, int startByteAdr, int bitAdr, bool bitValue)
        {
            byte[] bReceive = new byte[513];
            int varCount = 0;

            try
            {
                var value = new[] { bitValue ? (byte)1 : (byte)0};
                varCount = value.Length;
                // first create the header
                int packageSize = 35 + value.Length;
                Types.ByteArray package = new Types.ByteArray(packageSize);

                package.Add(new byte[] { 3, 0, 0 });
                package.Add((byte)packageSize);
                package.Add(new byte[] { 2, 0xf0, 0x80, 0x32, 1, 0, 0 });
                package.Add(Types.Word.ToByteArray((ushort)(varCount - 1)));
                package.Add(new byte[] { 0, 0x0e });
                package.Add(Types.Word.ToByteArray((ushort)(varCount + 4)));
                package.Add(new byte[] { 0x05, 0x01, 0x12, 0x0a, 0x10, 0x01 }); //ending 0x01 is used for writing a sinlge bit
                package.Add(Types.Word.ToByteArray((ushort)varCount));
                package.Add(Types.Word.ToByteArray((ushort)(db)));
                package.Add((byte)dataType);
                var overflow = (int)(startByteAdr * 8 / 0xffffU); // handles words with address bigger than 8191
                package.Add((byte)overflow);
                package.Add(Types.Word.ToByteArray((ushort)(startByteAdr * 8 + bitAdr)));
                package.Add(new byte[] { 0, 0x03 }); //ending 0x03 is used for writing a sinlge bit
                package.Add(Types.Word.ToByteArray((ushort)(varCount)));

                // now join the header and the data
                package.Add(value);

                _mSocket.Send(package.array, package.array.Length, SocketFlags.None);

                int numReceived = _mSocket.Receive(bReceive, 512, SocketFlags.None);
                if (bReceive[21] != 0xff)
                {
                    throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());
                }

                return ErrorCode.NoError;
            }
            catch (Exception exc)
            {
                LastErrorCode = ErrorCode.WriteData;
                LastErrorString = exc.Message;
                return LastErrorCode;
            }
        }

        /// <summary>
        /// Given a S7 variable type (Bool, Word, DWord, etc.), it converts the bytes in the appropriate C# format.
        /// </summary>
        /// <param name="varType"></param>
        /// <param name="bytes"></param>
        /// <param name="varCount"></param>
        /// <param name="bitAdr"></param>
        /// <returns></returns>
        private object ParseBytes(VarType varType, byte[] bytes, int varCount, byte bitAdr = 0)
        {
            if (bytes == null) return null;

            switch (varType)
            {
                case VarType.Byte:
                    if (varCount == 1)
                        return bytes[0];
                    else
                        return bytes;
                case VarType.Word:
                    if (varCount == 1)
                        return Types.Word.FromByteArray(bytes);
                    else
                        return Types.Word.ToArray(bytes);
                case VarType.Int:
                    if (varCount == 1)
                        return Types.Int.FromByteArray(bytes);
                    else
                        return Types.Int.ToArray(bytes);
                case VarType.DWord:
                    if (varCount == 1)
                        return Types.DWord.FromByteArray(bytes);
                    else
                        return Types.DWord.ToArray(bytes);
                case VarType.DInt:
                    if (varCount == 1)
                        return Types.DInt.FromByteArray(bytes);
                    else
                        return Types.DInt.ToArray(bytes);
                case VarType.Real:
                    if (varCount == 1)
                        return Types.Double.FromByteArray(bytes);
                    else
                        return Types.Double.ToArray(bytes);
                case VarType.String:
                    return Types.String.FromByteArray(bytes);
                case VarType.Timer:
                    if (varCount == 1)
                        return Types.Timer.FromByteArray(bytes);
                    else
                        return Types.Timer.ToArray(bytes);
                case VarType.Counter:
                    if (varCount == 1)
                        return Types.Counter.FromByteArray(bytes);
                    else
                        return Types.Counter.ToArray(bytes);
                case VarType.Bit:
                    if (varCount == 1)
                        if (bitAdr > 7)
                            return null;
                        else
                            return Types.Bit.FromByte(bytes[0], bitAdr);
                    else
                        return Types.Bit.ToBitArray(bytes);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Given a S7 variable type (Bool, Word, DWord, etc.), it returns how many bytes to read.
        /// </summary>
        /// <param name="varType"></param>
        /// <param name="varCount"></param>
        /// <returns></returns>
        private int VarTypeToByteLength(VarType varType, int varCount = 1)
        {
            switch (varType)
            {
                case VarType.Bit:
                    return varCount; //TODO
                case VarType.Byte:
                    return (varCount < 1) ? 1 : varCount;
                case VarType.String:
                    return varCount;
                case VarType.Word:
                case VarType.Timer:
                case VarType.Int:
                case VarType.Counter:
                    return varCount * 2;
                case VarType.DWord:
                case VarType.DInt:
                case VarType.Real:
                    return varCount * 4;
                default:
                    return 0;
            }
        }

        #region IDisposable members

        /// <summary>
        /// Releases all resources, disonnects from the plc and closes the socket
        /// </summary>
        public void Dispose()
        {
            if (_mSocket != null)
            {
                if (_mSocket.Connected)
                {
                    _mSocket.Shutdown(SocketShutdown.Both);
                    _mSocket.Close();
                }
            }
        }

        #endregion
    }
}
