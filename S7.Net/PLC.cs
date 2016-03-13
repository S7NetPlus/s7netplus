using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using S7.Net.Interfaces;
using Double = System.Double;

namespace S7.Net
{
    public class Plc : IPlc
    {
        private Socket _mSocket; //TCP connection to device

        public string IP { get; set; }
        public CpuType CPU { get; set; }
        public Int16 Rack { get; set; }
        public Int16 Slot { get; set; }
        public string Name { get; set; }
        public object Tag { get; set; }

        /// <summary>
        /// Pings the IP address and returns true if the result of the ping is Success.
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                using (Ping ping = new Ping())
                {
                    PingReply result;
                    try
                    {
                        result = ping.Send(IP);
                    }
                    catch (PingException)
                    {
                        result = null;
                    }
                    return result != null && result.Status == IPStatus.Success;
                }
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
                    return !((_mSocket.Poll(1000, SelectMode.SelectRead) && (_mSocket.Available == 0)) || !_mSocket.Connected);
                }
                catch { return false; }
            }
        }
        public string LastErrorString { get; private set; }
        public ErrorCode LastErrorCode { get; private set; }

        public Plc() : this(CpuType.S7400, "localhost", 0, 2) { }

        /// <summary>
        /// Creates a PLC object with all the parameters needed for connections.
        /// For S7-1200 and S7-1500, the default is rack = 0 and slot = 0.
        /// You need slot > 0 if you are connecting to external ethernet card (CP).
        /// For S7-300 and S7-400 the default is rack = 0 and slot = 2.
        /// </summary>
        /// <param name="cpu"></param>
        /// <param name="ip"></param>
        /// <param name="rack"></param>
        /// <param name="slot"></param>
        /// <param name="name"></param>
        /// <param name="tag"></param>
        public Plc(CpuType cpu, string ip, Int16 rack, Int16 slot, string name = "", object tag = null)
        {
            IP = ip;
            CPU = cpu;
            Rack = rack;
            Slot = slot;
            Name = name;
            Tag = tag;
        }

        public ErrorCode Open()
	    {
		    byte[] bReceive = new byte[256];

		    try 
            {
			    // check if available
                if (!IsAvailable)
                {
                    throw new Exception();
                }
            }
		    catch  
            {
			    LastErrorCode = ErrorCode.IPAddressNotAvailable;
			    LastErrorString = string.Format("Destination IP-Address '{0}' is not available!", IP);
			    return LastErrorCode;
		    }

		    try {
			    // open the channel
			    _mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			    _mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
			    _mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);

			    IPEndPoint server = new IPEndPoint(IPAddress.Parse(IP), 102);
			    _mSocket.Connect(server);
		    }
		    catch (Exception ex) {
			    LastErrorCode = ErrorCode.ConnectionError;
			    LastErrorString = ex.Message;
			    return ErrorCode.ConnectionError;
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
		    catch 
            {
			    LastErrorCode = ErrorCode.ConnectionError;
			    LastErrorString = string.Format("Couldn't establish the connection to {0}!", IP);
			    return ErrorCode.ConnectionError;
		    }

		    return ErrorCode.NoError;
	    }

	    public void Close()
	    {
		    if (_mSocket != null && _mSocket.Connected) 
            {
			    _mSocket.Close();
            }
	    }

        public byte[] ReadBytes(DataType dataType, int DB, int startByteAdr, int count)
        {
            byte[] bytes = new byte[count];

	        try
	        {
		        // first create the header
		        int packageSize = 31;
		        Types.ByteArray package = new Types.ByteArray(packageSize);

		        package.Add(new byte[] {0x03, 0x00, 0x00});
		        package.Add((byte) packageSize);
		        package.Add(new byte[]
		        {0x02, 0xf0, 0x80, 0x32, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x00, 0x04, 0x01, 0x12, 0x0a, 0x10});
		        // package.Add(0x02);  // datenart
		        switch (dataType)
		        {
			        case DataType.Timer:
			        case DataType.Counter:
				        package.Add((byte) dataType);
				        break;
			        default:
				        package.Add(0x02);
				        break;
		        }

		        package.Add(Types.Word.ToByteArray((ushort) (count)));
		        package.Add(Types.Word.ToByteArray((ushort) (DB)));
		        package.Add((byte) dataType);
                var overflow = (int)(startByteAdr * 8 / 0xffffU); // handles words with address bigger than 8191
                package.Add((byte)overflow);
                switch (dataType)
		        {
			        case DataType.Timer:
			        case DataType.Counter:
				        package.Add(Types.Word.ToByteArray((ushort) (startByteAdr)));
				        break;
			        default:
				        package.Add(Types.Word.ToByteArray((ushort) ((startByteAdr)*8)));
				        break;
		        }

		        _mSocket.Send(package.array, package.array.Length, SocketFlags.None);

		        byte[] bReceive = new byte[512];
		        int numReceived = _mSocket.Receive(bReceive, 512, SocketFlags.None);
		        if (bReceive[21] != 0xff) throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());

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
            catch(Exception exc)
            {
                LastErrorCode = ErrorCode.WriteData;
                LastErrorString = exc.Message;
                return null;
            }
        }

        public object Read(DataType dataType, int db, int startByteAdr, VarType varType, int varCount)
        {
            byte[] bytes = null;
            int cntBytes = 0;

            switch (varType)
            {
                case VarType.Byte:
                    cntBytes = varCount;
                    if (cntBytes < 1) cntBytes = 1;
                    bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);
                    if (bytes == null) return null;
                    if (varCount == 1)
                        return bytes[0];
                    else
                        return bytes;
                case VarType.Word:
                    cntBytes = varCount * 2;
                    bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);
                    if (bytes == null) return null;

                    if (varCount == 1)
                        return Types.Word.FromByteArray(bytes);
                    else
                        return Types.Word.ToArray(bytes);
                case VarType.Int:
                    cntBytes = varCount * 2;
                    bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);
                    if (bytes == null) return null;

                    if (varCount == 1)
                        return Types.Int.FromByteArray(bytes);
                    else
                        return Types.Int.ToArray(bytes);
                case VarType.DWord:
                    cntBytes = varCount * 4;
                    bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);
                    if (bytes == null) return null;

                    if (varCount == 1)
                        return Types.DWord.FromByteArray(bytes);
                    else
                        return Types.DWord.ToArray(bytes);
                case VarType.DInt:
                    cntBytes = varCount * 4;
                    bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);
                    if (bytes == null) return null;

                    if (varCount == 1)
                        return Types.DInt.FromByteArray(bytes);
                    else
                        return Types.DInt.ToArray(bytes);
                case VarType.Real:
                    cntBytes = varCount * 4;
                    bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);
                    if (bytes == null) return null;

                    if (varCount == 1)
                        return Types.Double.FromByteArray(bytes);
                    else
                        return Types.Double.ToArray(bytes);
                case VarType.String:
                    cntBytes = varCount;
                    bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);
                    if (bytes == null) return null;

                    return Types.String.FromByteArray(bytes);
                case VarType.Timer:
                    cntBytes = varCount * 2;
                    bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);
                    if (bytes == null) return null;

                    if (varCount == 1)
                        return Types.Timer.FromByteArray(bytes);
                    else
                        return Types.Timer.ToArray(bytes);
                case VarType.Counter:
                    cntBytes = varCount * 2;
                    bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);
                    if (bytes == null) return null;

                    if (varCount == 1)
                        return Types.Counter.FromByteArray(bytes);
                    else
                        return Types.Counter.ToArray(bytes);
                default:
                    return null;
            }
        }

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

        public object ReadStruct(Type structType, int db)
        {
            return ReadStruct(structType, db, 0);
        }

        public object ReadStruct(Type structType, int db, int startByteAdr)
        {
            int numBytes = Types.Struct.GetStructSize(structType);
            // now read the package
            List<byte> resultBytes = ReadMultipleBytes(numBytes, db, startByteAdr);

            // and decode it
            return Types.Struct.FromBytes(structType, resultBytes.ToArray());
        }

        /// <summary>
        /// Read a class from plc. Only properties are readed
        /// </summary>
        /// <param name="sourceClass">Instance of the class that will store the values</param>       
        /// <param name="db">Index of the DB; es.: 1 is for DB1</param>
        public void ReadClass(object sourceClass, int db)
        {
            ReadClass(sourceClass, db, 0);
        }

        public void ReadClass(object sourceClass, int db, int startByteAdr)
        {
            Type classType = sourceClass.GetType();
            int numBytes = Types.Class.GetClassSize(classType);
            // now read the package
            List<byte> resultBytes = ReadMultipleBytes(numBytes, db, startByteAdr);
            // and decode it
            Types.Class.FromBytes(sourceClass, classType, resultBytes.ToArray());
        }

        public ErrorCode WriteBytes(DataType dataType, int db, int startByteAdr, byte[] value)
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
                var overflow = (int) (startByteAdr*8/0xffffU); // handles words with address bigger than 8191
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
            catch
            {
                LastErrorCode = ErrorCode.WriteData;
                LastErrorString = "";
                return LastErrorCode;
            }
        }

        public object Write(DataType dataType, int db, int startByteAdr, object value)
        {
            byte[] package = null;

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

        public object Write(string variable, object value)
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
                                byte b = (byte)Read(DataType.DataBlock, mDB, mByte, VarType.Byte, 1);
                                if ((int)value == 1)
                                    b = (byte)(b | (byte)Math.Pow(2, mBit)); // Bit setzen
                                else
                                    b = (byte)(b & (b ^ (byte)Math.Pow(2, mBit))); // Bit rücksetzen

                                return Write(DataType.DataBlock, mDB, mByte, (byte)b);
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
            catch 
            {
                LastErrorCode = ErrorCode.WrongVarFormat;
                LastErrorString = "The variable'" + variable + "' could not be parsed. Please check the syntax and try again.";
                return LastErrorCode;
            }
        }

        public ErrorCode WriteStruct(object structValue, int db)
        {
            return WriteStruct(structValue, db, 0);
        }

        public ErrorCode WriteStruct(object structValue, int db, int startByteAdr)
        {
            var bytes = Types.Struct.ToBytes(structValue).ToList();
            var errCode = WriteMultipleBytes(bytes, db, startByteAdr);
            return errCode;
        }

        public ErrorCode WriteClass(object classValue, int db)
        {
            return WriteClass(classValue, db, 0);
        }

        public ErrorCode WriteClass(object classValue, int db, int startByteAdr)
        {
            var bytes = Types.Class.ToBytes(classValue).ToList();
            var errCode = WriteMultipleBytes(bytes, db, startByteAdr);
            return errCode;
        }

        /// <summary>
        /// Writes multiple bytes in a DB starting from index 0. This handles more than 200 bytes with multiple requests.
        /// </summary>
        /// <param name="bytes">The bytes to be written</param>
        /// <param name="db">The DB number</param>
        /// <returns>ErrorCode when writing (NoError if everything was ok)</returns>
        private ErrorCode WriteMultipleBytes(List<byte> bytes, int db)
        {
            return WriteMultipleBytes(bytes, db, 0);
        }

        private ErrorCode WriteMultipleBytes(List<byte> bytes, int db, int startByteAdr)
        {
            ErrorCode errCode = ErrorCode.NoError;
            int index = startByteAdr;
            try
            {
                while (bytes.Count > 0)
                {
                    var maxToWrite = Math.Min(bytes.Count, 200);
                    var part = bytes.ToList().GetRange(0, maxToWrite);
                    errCode = WriteBytes(DataType.DataBlock, db, index, part.ToArray());
                    bytes.RemoveRange(0, maxToWrite);
                    index += maxToWrite;
                    if (errCode != ErrorCode.NoError)
                    {
                        break;
                    }
                }
            }
            catch
            {
                LastErrorCode = ErrorCode.WriteData;
                LastErrorString = "An error occurred while writing data.";
            }
            return errCode;
        }

        /// <summary>
        /// Reads a number of bytes from a DB starting from index 0. This handles more than 200 bytes with multiple requests.
        /// </summary>
        /// <param name="numBytes"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private List<byte> ReadMultipleBytes(int numBytes, int db)
        {
            return ReadMultipleBytes(numBytes, db, 0);
        }

        private List<byte> ReadMultipleBytes(int numBytes, int db, int startByteAdr)
        {
            List<byte> resultBytes = new List<byte>();
            int index = startByteAdr;
            while (numBytes > 0)
            {
                var maxToRead = (int)Math.Min(numBytes, 200);
                byte[] bytes = (byte[])Read(DataType.DataBlock, db, index, VarType.Byte, (int)maxToRead);
                if (bytes == null)
                    return new List<byte>();
                resultBytes.AddRange(bytes);
                numBytes -= maxToRead;
                index += maxToRead;
            }
            return resultBytes;
        }



        #region IDisposable members

        public void Dispose()
        {
            if (_mSocket != null)
            {
                if (_mSocket.Connected)
                {
                    //Close() performs a Dispose on the socket.
                    _mSocket.Close();
                }
                //((IDisposable)_mSocket).Dispose();
            }
        } 

        #endregion
    }
}
