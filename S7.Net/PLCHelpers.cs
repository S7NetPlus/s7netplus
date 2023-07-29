using S7.Net.Helper;
using S7.Net.Protocol.S7;
using S7.Net.Types;
using System.Collections.Generic;
using System.Linq;
using DateTime = S7.Net.Types.DateTime;

namespace S7.Net
{
    public partial class Plc
    {
        private static void WriteTpktHeader(System.IO.MemoryStream stream, int length)
        {
            stream.Write(new byte[] { 0x03, 0x00 });
            stream.Write(Word.ToByteArray((ushort) length));
        }

        private static void WriteDataHeader(System.IO.MemoryStream stream)
        {
            stream.Write(new byte[] { 0x02, 0xf0, 0x80 });
        }

        private static void WriteS7Header(System.IO.MemoryStream stream, byte messageType, int parameterLength, int dataLength)
        {
            stream.WriteByte(0x32); // S7 protocol ID
            stream.WriteByte(messageType); // Message type
            stream.Write(new byte[] { 0x00, 0x00 }); // Reserved
            stream.Write(new byte[] { 0x00, 0x00 }); // PDU ref
            stream.Write(Word.ToByteArray((ushort) parameterLength));
            stream.Write(Word.ToByteArray((ushort) dataLength));
        }

        /// <summary>
        /// Creates the header to read bytes from the PLC.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="amount">The number of items to read.</param>
        private static void WriteReadHeader(System.IO.MemoryStream stream, int amount = 1)
        {
            // Header size 19, 12 bytes per item
            WriteTpktHeader(stream, 19 + 12 * amount);
            WriteDataHeader(stream);
            WriteS7Header(stream, 0x01, 2 + 12 * amount, 0);
            // Function code: read request
            stream.WriteByte(0x04);
            //amount of requests
            stream.WriteByte((byte)amount);
        }

        private static void WriteUserDataHeader(System.IO.MemoryStream stream, int parameterLength, int dataLength)
        {
            const byte s7MessageTypeUserData = 0x07;

            WriteTpktHeader(stream, 17 + parameterLength + dataLength);
            WriteDataHeader(stream);
            WriteS7Header(stream, s7MessageTypeUserData, parameterLength, dataLength);
        }

        private static void WriteSzlReadRequest(System.IO.MemoryStream stream, ushort szlId, ushort szlIndex)
        {
            WriteUserDataHeader(stream, 8, 8);

            // Parameter
            const byte szlMethodRequest = 0x11;
            const byte szlTypeRequest = 0b100;
            const byte szlFunctionGroupCpuFunctions = 0b100;
            const byte subFunctionReadSzl = 0x01;

            // Parameter head
            stream.Write(new byte[] { 0x00, 0x01, 0x12 });
            // Parameter length
            stream.WriteByte(0x04);
            // Method
            stream.WriteByte(szlMethodRequest);
            // Type / function group
            stream.WriteByte(szlTypeRequest << 4 | szlFunctionGroupCpuFunctions);
            // Subfunction
            stream.WriteByte(subFunctionReadSzl);
            // Sequence number
            stream.WriteByte(0);

            // Data
            const byte success = 0xff;
            const byte transportSizeOctetString = 0x09;

            // Return code
            stream.WriteByte(success);
            // Transport size
            stream.WriteByte(transportSizeOctetString);
            // Length
            stream.Write(Word.ToByteArray(4));
            // SZL-ID
            stream.Write(Word.ToByteArray(szlId));
            // SZL-Index
            stream.Write(Word.ToByteArray(szlIndex));
        }

        /// <summary>
        /// Create the bytes-package to request data from the PLC. You have to specify the memory type (dataType),
        /// the address of the memory, the address of the byte and the bytes count.
        /// </summary>
        /// <param name="stream">The stream to write the read data request to.</param>
        /// <param name="dataType">MemoryType (DB, Timer, Counter, etc.)</param>
        /// <param name="db">Address of the memory to be read</param>
        /// <param name="startByteAdr">Start address of the byte</param>
        /// <param name="count">Number of bytes to be read</param>
        /// <returns></returns>
        private static void BuildReadDataRequestPackage(System.IO.MemoryStream stream, DataType dataType, int db, int startByteAdr, int count = 1)
        {
            //single data req = 12
            stream.Write(new byte[] { 0x12, 0x0a, 0x10 });
            switch (dataType)
            {
                case DataType.Timer:
                case DataType.Counter:
                    stream.WriteByte((byte)dataType);
                    break;
                default:
                    stream.WriteByte(0x02);
                    break;
            }

            stream.Write(Word.ToByteArray((ushort)(count)));
            stream.Write(Word.ToByteArray((ushort)(db)));
            stream.WriteByte((byte)dataType);
            var overflow = (int)(startByteAdr * 8 / 0xffffU); // handles words with address bigger than 8191
            stream.WriteByte((byte)overflow);
            switch (dataType)
            {
                case DataType.Timer:
                case DataType.Counter:
                    stream.Write(Word.ToByteArray((ushort)(startByteAdr)));
                    break;
                default:
                    stream.Write(Word.ToByteArray((ushort)((startByteAdr) * 8)));
                    break;
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
        private object? ParseBytes(VarType varType, byte[] bytes, int varCount, byte bitAdr = 0)
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
                        return Types.Real.FromByteArray(bytes);
                    else
                        return Types.Real.ToArray(bytes);
                case VarType.LReal:
                    if (varCount == 1)
                        return Types.LReal.FromByteArray(bytes);
                    else
                        return Types.LReal.ToArray(bytes);

                case VarType.String:
                    return Types.String.FromByteArray(bytes);
                case VarType.S7String:
                    return S7String.FromByteArray(bytes);
                case VarType.S7WString:
                    return S7WString.FromByteArray(bytes);

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
                        return Bit.ToBitArray(bytes, varCount);
                    }
                case VarType.DateTime:
                    if (varCount == 1)
                    {
                        return DateTime.FromByteArray(bytes);
                    }
                    else
                    {
                        return DateTime.ToArray(bytes);
                    }
                case VarType.DateTimeLong:
                    if (varCount == 1)
                    {
                        return DateTimeLong.FromByteArray(bytes);
                    }
                    else
                    {
                        return DateTimeLong.ToArray(bytes);
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Given a S7 <see cref="VarType"/> (Bool, Word, DWord, etc.), it returns how many bytes to read.
        /// </summary>
        /// <param name="varType"></param>
        /// <param name="varCount"></param>
        /// <returns>Byte lenght of variable</returns>
        internal static int VarTypeToByteLength(VarType varType, int varCount = 1)
        {
            switch (varType)
            {
                case VarType.Bit:
                    return (varCount + 7) / 8;
                case VarType.Byte:
                    return (varCount < 1) ? 1 : varCount;
                case VarType.String:
                    return varCount;
                case VarType.S7String:
                    return ((varCount + 2) & 1) == 1 ? (varCount + 3) : (varCount + 2);
                case VarType.S7WString:
                    return (varCount * 2) + 4;
                case VarType.Word:
                case VarType.Timer:
                case VarType.Int:
                case VarType.Counter:
                    return varCount * 2;
                case VarType.DWord:
                case VarType.DInt:
                case VarType.Real:
                    return varCount * 4;
                case VarType.LReal:
                case VarType.DateTime:
                    return varCount * 8;
                case VarType.DateTimeLong:
                    return varCount * 12;
                default:
                    return 0;
            }
        }

        private byte[] GetS7ConnectionSetup()
        {
            return new byte[] {  3, 0, 0, 25, 2, 240, 128, 50, 1, 0, 0, 255, 255, 0, 8, 0, 0, 240, 0, 0, 3, 0, 3,
                    3, 192 // Use 960 PDU size
            };
        }

        private void ParseDataIntoDataItems(byte[] s7data, List<DataItem> dataItems)
        {
            int offset = 14;
            foreach (var dataItem in dataItems)
            {
                // check for Return Code = Success
                if (s7data[offset] != 0xff)
                    throw new PlcException(ErrorCode.WrongNumberReceivedBytes);

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

                // Always align to even offset
                if (offset % 2 != 0)
                    offset++;
            }
        }

        private static byte[] BuildReadRequestPackage(IList<DataItemAddress> dataItems)
        {
            int packageSize = 19 + (dataItems.Count * 12);
            var package = new System.IO.MemoryStream(packageSize);

            WriteReadHeader(package, dataItems.Count);

            foreach (var dataItem in dataItems)
            {
                BuildReadDataRequestPackage(package, dataItem.DataType, dataItem.DB, dataItem.StartByteAddress, dataItem.ByteLength);
            }

            return package.ToArray();
        }

        private static byte[] BuildSzlReadRequestPackage(ushort szlId, ushort szlIndex)
        {
            var stream = new System.IO.MemoryStream();
            
            WriteSzlReadRequest(stream, szlId, szlIndex);
            stream.SetLength(stream.Position);

            return stream.ToArray();
        }
    }
}
