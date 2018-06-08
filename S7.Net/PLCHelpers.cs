using S7.Net.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace S7.Net
{

    internal class PLCAddress
    {
        public DataType dataType;
        public int DBNumber;
        public int Address;
        public int BitNumber;
        public VarType varType;

        public PLCAddress(string address)
        {
            ParseString(address);
        }

        private void ParseString(string address)
        {
            BitNumber = -1;
            switch (address.Substring(0, 2))
            {
                case "DB":
                    string[] strings = address.Split(new char[] { '.' });
                    if (strings.Length < 2)
                        throw new InvalidAddressException("To few periods for DB address");

                    dataType = DataType.DataBlock;
                    DBNumber = int.Parse(strings[0].Substring(2));
                    Address = int.Parse(strings[1].Substring(3));

                    string dbType = strings[1].Substring(0, 3);
                    switch (dbType)
                    {
                        case "DBB":
                            varType = VarType.Byte;
                            return;
                        case "DBW":
                            varType = VarType.Word;
                            return;
                        case "DBD":
                            varType = VarType.DWord;
                            return;
                        case "DBX":
                            BitNumber = int.Parse(strings[2]);
                            if (BitNumber > 7)
                                throw new InvalidAddressException("Bit can only be 0-7");
                            varType = VarType.Bit;
                            return;
                        default:
                            throw new InvalidAddressException();
                    }
                case "EB":
                    // Input byte
                    dataType = DataType.Input;
                    DBNumber = 0;
                    Address = int.Parse(address.Substring(2));
                    varType = VarType.Byte;
                    return;
                case "EW":
                    // Input word
                    dataType = DataType.Input;
                    DBNumber = 0;
                    Address = int.Parse(address.Substring(2));
                    varType = VarType.Word;
                    return;
                case "ED":
                    // Input double-word
                    dataType = DataType.Input;
                    DBNumber = 0;
                    Address = int.Parse(address.Substring(2));
                    varType = VarType.DWord;
                    return;
                case "AB":
                    // Output byte
                    dataType = DataType.Output;
                    DBNumber = 0;
                    Address = int.Parse(address.Substring(2));
                    varType = VarType.Byte;
                    return;
                case "AW":
                    // Output word
                    dataType = DataType.Output;
                    DBNumber = 0;
                    Address = int.Parse(address.Substring(2));
                    varType = VarType.Word;
                    return;
                case "AD":
                    // Output double-word
                    dataType = DataType.Output;
                    DBNumber = 0;
                    Address = int.Parse(address.Substring(2));
                    varType = VarType.DWord;
                    return;
                case "MB":
                    // Memory byte
                    dataType = DataType.Memory;
                    DBNumber = 0;
                    Address = int.Parse(address.Substring(2));
                    varType = VarType.Byte;
                    return;
                case "MW":
                    // Memory word
                    dataType = DataType.Memory;
                    DBNumber = 0;
                    Address = int.Parse(address.Substring(2));
                    varType = VarType.Word;
                    return;
                case "MD":
                    // Memory double-word
                    dataType = DataType.Memory;
                    DBNumber = 0;
                    Address = int.Parse(address.Substring(2));
                    varType = VarType.DWord;
                    return;
                default:
                    switch (address.Substring(0, 1))
                    {
                        case "E":
                        case "I":
                            // Input
                            dataType = DataType.Input;
                            break;
                        case "A":
                        case "O":
                            // Output
                            dataType = DataType.Output;
                            break;
                        case "M":
                            // Memory
                            dataType = DataType.Memory;
                            break;
                        case "T":
                            // Timer
                            dataType = DataType.Timer;
                            DBNumber = 0;
                            Address = int.Parse(address.Substring(1));
                            varType = VarType.Timer;
                            return;
                        case "Z":
                        case "C":
                            // Counter
                            dataType = DataType.Timer;
                            DBNumber = 0;
                            Address = int.Parse(address.Substring(1));
                            varType = VarType.Counter;
                            return;
                        default:
                            throw new InvalidAddressException(string.Format("{0} is not a valid address", address.Substring(0, 1)));
                    }

                    string txt2 = address.Substring(1);
                    if (txt2.IndexOf(".") == -1)
                        throw new InvalidAddressException("To few periods for DB address");

                    Address = int.Parse(txt2.Substring(0, txt2.IndexOf(".")));
                    BitNumber = int.Parse(txt2.Substring(txt2.IndexOf(".") + 1));
                    if (BitNumber > 7)
                        throw new InvalidAddressException("Bit can only be 0-7");
                    return;
            }
        }
    }

    public partial class Plc
    {
        /// <summary>
        /// Creates the header to read bytes from the PLC
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        private ByteArray ReadHeaderPackage(int amount = 1)
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
        /// Create the bytes-package to request data from the PLC. You have to specify the memory type (dataType), 
        /// the address of the memory, the address of the byte and the bytes count. 
        /// </summary>
        /// <param name="dataType">MemoryType (DB, Timer, Counter, etc.)</param>
        /// <param name="db">Address of the memory to be read</param>
        /// <param name="startByteAdr">Start address of the byte</param>
        /// <param name="count">Number of bytes to be read</param>
        /// <returns></returns>
        private ByteArray CreateReadDataRequestPackage(DataType dataType, int db, int startByteAdr, int count = 1)
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

            package.Add(Word.ToByteArray((ushort)(count)));
            package.Add(Word.ToByteArray((ushort)(db)));
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
            if (bytes == null || bytes.Length == 0)
                return null;

            switch (varType)
            {
                case VarType.Byte:
                    if (varCount == 1)
                        return bytes[0];
                    else
                        return bytes;
                case VarType.Word:
                    if (varCount == 1)
                        return Word.FromByteArray(bytes);
                    else
                        return Word.ToArray(bytes);
                case VarType.Int:
                    if (varCount == 1)
                        return Int.FromByteArray(bytes);
                    else
                        return Int.ToArray(bytes);
                case VarType.DWord:
                    if (varCount == 1)
                        return DWord.FromByteArray(bytes);
                    else
                        return DWord.ToArray(bytes);
                case VarType.DInt:
                    if (varCount == 1)
                        return DInt.FromByteArray(bytes);
                    else
                        return DInt.ToArray(bytes);
                case VarType.Real:
                    if (varCount == 1)
                        return Types.Double.FromByteArray(bytes);
                    else
                        return Types.Double.ToArray(bytes);

                case VarType.String:
                    return Types.String.FromByteArray(bytes);
                case VarType.StringEx:
                    return StringEx.FromByteArray(bytes);

                case VarType.Timer:
                    if (varCount == 1)
                        return Timer.FromByteArray(bytes);
                    else
                        return Timer.ToArray(bytes);
                case VarType.Counter:
                    if (varCount == 1)
                        return Counter.FromByteArray(bytes);
                    else
                        return Counter.ToArray(bytes);
                case VarType.Bit:
                    if (varCount == 1)
                    {
                        if (bitAdr > 7)
                            return null;
                        else
                            return Bit.FromByte(bytes[0], bitAdr);
                    }
                    else
                    {
                        return Bit.ToBitArray(bytes);
                    }
                default:
                    return null;
            }
        }

        public byte[] GetPackage(object value)
        {
            switch (value.GetType().Name)
            {
                case "Byte":
                    return Types.Byte.ToByteArray((byte)value);
                case "Int16":
                    return Types.Int.ToByteArray((Int16)value);
                case "UInt16":
                    return Types.Word.ToByteArray((UInt16)value);
                case "Int32":
                    return Types.DInt.ToByteArray((Int32)value);
                case "UInt32":
                    return Types.DWord.ToByteArray((UInt32)value);
                case "Double":
                    return Types.Double.ToByteArray((double)value);
                case "Byte[]":
                    return (byte[])value;
                case "Int16[]":
                    return Types.Int.ToByteArray((Int16[])value);
                case "UInt16[]":
                    return Types.Word.ToByteArray((UInt16[])value);
                case "Int32[]":
                    return Types.DInt.ToByteArray((Int32[])value);
                case "UInt32[]":
                    return Types.DWord.ToByteArray((UInt32[])value);
                case "Double[]":
                    return Types.Double.ToByteArray((double[])value);
                case "String":
                    return Types.String.ToByteArray(value as string);
                default:
                    throw new InvalidVariableTypeException();
            }
        }


        /// <summary>
        /// Sets the <see cref="LastErrorCode"/> to <see cref="ErrorCode.NoError"/> and <see cref="LastErrorString"/> to <see cref="string.Empty"/>.
        /// </summary>
        public void ClearLastError()
        {
            LastErrorCode = ErrorCode.NoError;
            LastErrorString = string.Empty;
        }

        /// <summary>
        /// Given a S7 <see cref="VarType"/> (Bool, Word, DWord, etc.), it returns how many bytes to read.
        /// </summary>
        /// <param name="varType"></param>
        /// <param name="varCount"></param>
        /// <returns>Byte lenght of variable</returns>
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
                case VarType.StringEx:
                    return varCount + 2;
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

        private byte[] GetCOPTConnectionRequest(CpuType CPU)
        {
            byte[] bSend1 = {
                    3, 0, 0, 22, //TPKT
                    17,     //COTP Header Length
                    224,    //Connect Request 
                    0, 0,   //Destination Reference
                    0, 46,  //Source Reference
                    0,      //Flags
                    193,    //Parameter Code (src-tasp)
                    2,      //Parameter Length
                    1, 0,   //Source TASP
                    194,    //Parameter Code (dst-tasp)
                    2,      //Parameter Length
                    3, 0,   //Destination TASP
                    192,    //Parameter Code (tpdu-size)
                    1,      //Parameter Length
                    9       //TPDU Size (2^9 = 512)
                };

            switch (CPU)
            {
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
                    throw new Exception("Wrong CPU Type Secified");
            }
            return bSend1;
        }

        private byte[] GetS7ConnectionSetup()
        {
            return new byte[] {  3, 0, 0, 25, 2, 240, 128, 50, 1, 0, 0, 255, 255, 0, 8, 0, 0, 240, 0, 0, 3, 0, 3,
                    7, 80 //Try 1920 PDU Size. Same as libnodave.
            };
        }

        private void ParseDataIntoDataItems(byte[] s7data, List<DataItem> dataItems)
        {
            int offset = 14;
            foreach (var dataItem in dataItems)
            {
                // check for Return Code = Success
                if (s7data[offset] != 0xff)
                    throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());

                // to Data bytes
                offset += 4;

                int byteCnt = VarTypeToByteLength(dataItem.VarType, dataItem.Count);
                dataItem.Value = ParseBytes(
                    dataItem.VarType,
                    s7data.Skip(offset).Take(byteCnt).ToArray(),
                    dataItem.Count,
                    dataItem.BitAdr
                );

                // next Item
                offset += byteCnt;

                // Fill byte in response when bytecount is odd
                if (dataItem.Count % 2 != 0 && (dataItem.VarType == VarType.Byte || dataItem.VarType == VarType.Bit))
                    offset++;
            }
        }
    }
}
