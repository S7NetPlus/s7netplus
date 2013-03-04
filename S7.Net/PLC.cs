using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace S7
{
    public class Plc : IPlc
    {
        private Socket _mSocket; //TCP connection to device

        public string IP
        { get; set; }

        public CpuType CPU
        { get; set; }

        public Int16 Rack
        { get; set; }

        public Int16 Slot
        { get; set; }

        public string Name
        { get; set; }

        public object Tag
        { get; set; }

        public bool IsAvailable
        {
            get
            {
                Ping ping = new Ping();
                PingReply result = ping.Send(IP);
                return result != null && result.Status == IPStatus.Success;
            }
        }

        public bool IsConnected { get; private set; }

        public string LastErrorString { get; private set; }
        public ErrorCode LastErrorCode { get; private set; }

        public Plc() : this(CpuType.S7400, "localhost", 0, 2) { }

        public Plc(CpuType cpu, string ip, Int16 rack, Int16 slot, string name = "", object tag = null)
        {
            IsConnected = false;
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

		    try {
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
			    IsConnected = true;
		    }
		    catch 
            {
			    LastErrorCode = ErrorCode.ConnectionError;
			    LastErrorString = "Couldn't establish the connection!";
			    IsConnected = false;
			    return ErrorCode.ConnectionError;
		    }

		    return ErrorCode.NoError;
	    }

	    public void Close()
	    {
		    if (_mSocket != null && _mSocket.Connected) {
			    _mSocket.Close();
                IsConnected = false;
		    }
	    }

        public byte[] ReadBytes(DataType DataType, int DB, int StartByteAdr, int count)
        {
            byte[] bytes = new byte[count];

            try
            {
                // first create the header
                int packageSize = 31;
                Types.ByteArray package = new Types.ByteArray(packageSize);

                package.Add(new byte[] { 0x03, 0x00, 0x00 });
                package.Add((byte)packageSize);
                package.Add(new byte[] { 0x02, 0xf0, 0x80, 0x32, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x00, 0x04, 0x01, 0x12, 0x0a, 0x10});
               // package.Add(0x02);  // datenart
                switch (DataType)
                {
                    case DataType.Timer:
                    case DataType.Counter:
                        package.Add((byte)DataType);
                        break;
                    default:
                        package.Add(0x02);
                        break;
                }

                package.Add(Types.Word.ToByteArray((ushort)(count)));
                package.Add(Types.Word.ToByteArray((ushort)(DB)));
                package.Add((byte)DataType);
                package.Add((byte)0);
                switch (DataType)
                {
                    case DataType.Timer:
                    case DataType.Counter:
                        package.Add(Types.Word.ToByteArray((ushort)(StartByteAdr)));
                        break;
                    default:
                        package.Add(Types.Word.ToByteArray((ushort)((StartByteAdr) * 8)));
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
            catch
            {
                LastErrorCode = ErrorCode.WriteData;
                LastErrorString = "";
                return null;
            }
        }

        public object Read(DataType DataType, int DB, int StartByteAdr, VarType VarType, int VarCount)
        {
            byte[] bytes = null;
            int cntBytes = 0;

            switch (VarType)
            {
                case VarType.Byte:
                    cntBytes = VarCount;
                    if (cntBytes < 1) cntBytes = 1;
                    bytes = ReadBytes(DataType, DB, StartByteAdr, cntBytes);
                    if (bytes == null) return null;
                    if (VarCount == 1)
                        return bytes[0];
                    else
                        return bytes;
                case VarType.Word:
                    cntBytes = VarCount * 2;
                    bytes = ReadBytes(DataType, DB, StartByteAdr, cntBytes);
                    if (bytes == null) return null;

                    if (VarCount == 1)
                        return Types.Word.FromByteArray(bytes);
                    else
                        return Types.Word.ToArray(bytes);
                case VarType.Int:
                    cntBytes = VarCount * 2;
                    bytes = ReadBytes(DataType, DB, StartByteAdr, cntBytes);
                    if (bytes == null) return null;

                    if (VarCount == 1)
                        return Types.Int.FromByteArray(bytes);
                    else
                        return Types.Int.ToArray(bytes);
                case VarType.DWord:
                    cntBytes = VarCount * 4;
                    bytes = ReadBytes(DataType, DB, StartByteAdr, cntBytes);
                    if (bytes == null) return null;

                    if (VarCount == 1)
                        return Types.DWord.FromByteArray(bytes);
                    else
                        return Types.DWord.ToArray(bytes);
                case VarType.DInt:
                    cntBytes = VarCount * 4;
                    bytes = ReadBytes(DataType, DB, StartByteAdr, cntBytes);
                    if (bytes == null) return null;

                    if (VarCount == 1)
                        return Types.DInt.FromByteArray(bytes);
                    else
                        return Types.DInt.ToArray(bytes);
                case VarType.Real:
                    cntBytes = VarCount * 4;
                    bytes = ReadBytes(DataType, DB, StartByteAdr, cntBytes);
                    if (bytes == null) return null;

                    if (VarCount == 1)
                        return Types.Double.FromByteArray(bytes);
                    else
                        return Types.Double.ToArray(bytes);
                case VarType.String:
                    cntBytes = VarCount;
                    bytes = ReadBytes(DataType, DB, StartByteAdr, cntBytes);
                    if (bytes == null) return null;

                    return Types.String.FromByteArray(bytes);
                case VarType.Timer:
                    cntBytes = VarCount * 2;
                    bytes = ReadBytes(DataType, DB, StartByteAdr, cntBytes);
                    if (bytes == null) return null;

                    if (VarCount == 1)
                        return Types.Timer.FromByteArray(bytes);
                    else
                        return Types.Timer.ToArray(bytes);
                case VarType.Counter:
                    cntBytes = VarCount * 2;
                    bytes = ReadBytes(DataType, DB, StartByteAdr, cntBytes);
                    if (bytes == null) return null;

                    if (VarCount == 1)
                        return Types.Counter.FromByteArray(bytes);
                    else
                        return Types.Counter.ToArray(bytes);
                default:
                    return null;
            }
            return null;
        }

        object IPlc.Read(string variable)
        {
            DataType mDataType;
            int mDB;
            int mByte;
            int mBit;

            byte objByte;
            UInt16 objUInt16;
            UInt32 objUInt32;
            double objDouble;
            bool[] objBoolArray;

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
                        mDataType = DataType.DataBlock;
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
                                objBoolArray = (bool[])Read(DataType.DataBlock, mDB, mByte, VarType.Bit, 1);
                                return objBoolArray[mBit];
                            default:
                                throw new Exception();
                        }
                    case "EB":
                        // Eingangsbyte
                        objByte = (byte)Read(DataType.Input, 0, int.Parse(txt.Substring(2)), VarType.Byte, 1);
                        return objByte;
                    case "EW":
                        // Eingangswort
                        objUInt16 = (UInt16)Read(DataType.Input, 0, int.Parse(txt.Substring(2)), VarType.Word, 1);
                        return objUInt16;
                    case "ED":
                        // Eingangsdoppelwort
                        objUInt32 = (UInt32)Read(DataType.Input, 0, int.Parse(txt.Substring(2)), VarType.DWord, 1);
                        return objUInt32;
                    case "AB":
                        // Ausgangsbyte
                        objByte = (byte)Read(DataType.Output, 0, int.Parse(txt.Substring(2)), VarType.Byte, 1);
                        return objByte;
                    case "AW":
                        // Ausgangswort
                        objUInt16 = (UInt16)Read(DataType.Output, 0, int.Parse(txt.Substring(2)), VarType.Word, 1);
                        return objUInt16;
                    case "AD":
                        // Ausgangsdoppelwort
                        objUInt32 = (UInt32)Read(DataType.Output, 0, int.Parse(txt.Substring(2)), VarType.DWord, 1);
                        return objUInt32;
                    case "MB":
                        // Merkerbyte
                        objByte = (byte)Read(DataType.Marker, 0, int.Parse(txt.Substring(2)), VarType.Byte, 1);
                        return objByte;
                    case "MW":
                        // Merkerwort
                        objUInt16 = (UInt16)Read(DataType.Marker, 0, int.Parse(txt.Substring(2)), VarType.Word, 1);
                        return objUInt16;
                    case "MD":
                        // Merkerdoppelwort
                        objUInt32 = (UInt32)Read(DataType.Marker, 0, int.Parse(txt.Substring(2)), VarType.DWord, 1);
                        return objUInt32;
                    default:
                        switch (txt.Substring(0, 1))
                        {
                            case "E":
                            case "I":
                                // Eingang
                                mDataType = DataType.Input;
                                break;
                            case "A":
                            case "O":
                                // Ausgang
                                mDataType = DataType.Output;
                                break;
                            case "M":
                                // Merker
                                mDataType = DataType.Marker;
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
                        objBoolArray = (bool[])Read(mDataType, 0, mByte, VarType.Bit, 1);
                        return objBoolArray[mBit];
                }
            }
            catch 
            {
                LastErrorCode = ErrorCode.WrongVarFormat;
                LastErrorString = "Die Variable '" + variable + "' konnte nicht entschlüsselt werden!";
                return LastErrorCode;
            }
        }

        public object ReadStruct(Type structType, int DB)
        {
            double numBytes = Types.Struct.GetStructSize(structType);
            // now read the package
            byte[] bytes = (byte[])Read(DataType.DataBlock, DB, 0, VarType.Byte, (int)numBytes);
            // and decode it
            return Types.Struct.FromBytes(structType, bytes);
        }

        public ErrorCode WriteBytes(DataType DataType, int DB, int StartByteAdr, byte[] value)
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
                package.Add(Types.Word.ToByteArray((ushort)(DB)));
                package.Add((byte)DataType);
                package.Add((byte)0);
                package.Add(Types.Word.ToByteArray((ushort)(StartByteAdr * 8)));
                package.Add(new byte[] { 0, 4 });
                package.Add(Types.Word.ToByteArray((ushort)(varCount * 8)));

                // now join the header and the data
                package.Add(value);

                _mSocket.Send(package.array, package.array.Length, SocketFlags.None);

                int numReceived = _mSocket.Receive(bReceive, 512, SocketFlags.None);
                if (bReceive[21] != 0xff) throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());

                return ErrorCode.NoError;
            }
            catch
            {
                LastErrorCode = ErrorCode.WriteData;
                LastErrorString = "";
                return LastErrorCode;
            }
        }

        public object Write(DataType DataType, int DB, int StartByteAdr, object value)
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
            return WriteBytes(DataType, DB, StartByteAdr, package);
        }

        public object Write(string variable, object value)
        {
            DataType mDataType;
            int mDB;
            int mByte;
            int mBit;

            string txt2;
            byte _byte;
            object objValue;

            string txt = variable.ToUpper();
            txt = txt.Replace(" ", ""); // Leerzeichen entfernen 

            try
            {
                switch (txt.Substring(0, 2))
                {
                    case "DB":
                        string[] strings = txt.Split(new char[]{'.'});
                        if (strings.Length < 2)
                            throw new Exception();

                        mDB = int.Parse(strings[0].Substring(2));
                        mDataType = DataType.DataBlock;
                        string dbType = strings[1].Substring(0, 3);
                        int dbIndex = int.Parse(strings[1].Substring(3));                       
                       
                        switch (dbType)
                        {
                            case "DBB":
                                objValue = Convert.ChangeType(value, typeof(byte));
                                return Write(DataType.DataBlock, mDB, dbIndex, (byte)objValue);
                            case "DBW":
                                objValue = Convert.ChangeType(value, typeof(UInt16));
                                return Write(DataType.DataBlock, mDB, dbIndex, (UInt16)objValue);
                            case "DBD":
                                objValue = Convert.ChangeType(value, typeof(UInt32));
                                return Write(DataType.DataBlock, mDB, dbIndex, (UInt32)objValue);
                            case "DBX":
                                mByte = dbIndex;
                                mBit = int.Parse(strings[2]);
                                if (mBit > 7) throw new Exception();
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
                                throw new Exception();
                        }
                    case "EB":
                        // Eingangsbyte
                        objValue = Convert.ChangeType(value, typeof(byte));
                        return Write(DataType.Input, 0, int.Parse(txt.Substring(2)), (byte)objValue);
                    case "EW":
                        // Eingangswort
                        objValue = Convert.ChangeType(value, typeof(UInt16));
                        return Write(DataType.Input, 0, int.Parse(txt.Substring(2)), (UInt16)objValue);
                    case "ED":
                        // Eingangsdoppelwort
                        objValue = Convert.ChangeType(value, typeof(UInt32));
                        return Write(DataType.Input, 0, int.Parse(txt.Substring(2)), (UInt32)objValue);
                    case "AB":
                        // Ausgangsbyte
                        objValue = Convert.ChangeType(value, typeof(byte));
                        return Write(DataType.Output, 0, int.Parse(txt.Substring(2)), (byte)objValue);
                    case "AW":
                        // Ausgangswort
                        objValue = Convert.ChangeType(value, typeof(UInt16));
                        return Write(DataType.Output, 0, int.Parse(txt.Substring(2)), (UInt16)objValue);
                    case "AD":
                        // Ausgangsdoppelwort
                        objValue = Convert.ChangeType(value, typeof(UInt32));
                        return Write(DataType.Output, 0, int.Parse(txt.Substring(2)), (UInt32)objValue);
                    case "MB":
                        // Merkerbyte
                        objValue = Convert.ChangeType(value, typeof(byte));
                        return Write(DataType.Marker, 0, int.Parse(txt.Substring(2)), (byte)objValue);
                    case "MW":
                        // Merkerwort
                        objValue = Convert.ChangeType(value, typeof(UInt16));
                        return Write(DataType.Marker, 0, int.Parse(txt.Substring(2)), (UInt16)objValue);
                    case "MD":
                        // Merkerdoppelwort
                        return Write(DataType.Marker, 0, int.Parse(txt.Substring(2)), value);
                    default:
                        switch (txt.Substring(0, 1))
                        {
                            case "E":
                            case "I":
                                // Eingang
                                mDataType = DataType.Input;
                                break;
                            case "A":
                            case "O":
                                // Ausgang
                                mDataType = DataType.Output;
                                break;
                            case "M":
                                // Merker
                                mDataType = DataType.Marker;
                                break;
                            case "T":
                                // Timer
                                return Write(DataType.Timer, 0, int.Parse(txt.Substring(1)), (double)value);
                            case "Z":
                            case "C":
                                // Zähler
                                return Write(DataType.Counter, 0, int.Parse(txt.Substring(1)), (short)value);
                            default:
                                throw new Exception("Unbekannte Variable");
                        }

                        txt2 = txt.Substring(1);
                        if (txt2.IndexOf(".") == -1) throw new Exception("Unbekannte Variable");

                        mByte = int.Parse(txt2.Substring(0, txt2.IndexOf(".")));
                        mBit = int.Parse(txt2.Substring(txt2.IndexOf(".") + 1));
                        if (mBit > 7) throw new Exception("Unbekannte Variable");
                        _byte = (byte)Read(mDataType, 0, mByte, VarType.Byte, 1);
                        if ((int)value == 1)
                            _byte = (byte)(_byte | (byte)Math.Pow(2, mBit));      // Bit setzen
                        else
                            _byte = (byte)(_byte & (_byte ^ (byte)Math.Pow(2, mBit))); // Bit rücksetzen

                        return Write(mDataType, 0, mByte, (byte)_byte);
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                LastErrorCode = ErrorCode.WrongVarFormat;
                LastErrorString = "Die Variable '" + variable + "' konnte nicht entschlüsselt werden!";
                return LastErrorCode;
            }
        }

        public ErrorCode WriteStruct(object structValue, int DB)
        {
            try
            {
                byte[] bytes = Types.Struct.ToBytes(structValue);
                ErrorCode errCode = WriteBytes(DataType.DataBlock, DB, 0, bytes);
                return errCode;
            }
            catch
            {
                LastErrorCode = ErrorCode.WriteData;
                LastErrorString = "Fehler beim Schreiben der Daten aufgetreten!";
                return LastErrorCode;
            }
        }

        public void Dispose()
        {
            if (_mSocket != null)
            {
                ((IDisposable)_mSocket).Dispose();
            }
        }
    }
}
