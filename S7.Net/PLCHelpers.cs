﻿using S7.Net.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using DateTime = S7.Net.Types.DateTime;

namespace S7.Net
{
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
            package.Add(new byte[] { 0x03, 0x00 });
            //complete package size
            package.Add(Types.Int.ToByteArray((short)(19 + (12 * amount))));
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
                        return Types.Single.FromByteArray(bytes);
                    else
                        return Types.Single.ToArray(bytes);

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
        private int VarTypeToByteLength(VarType varType, int varCount = 1)
        {
            switch (varType)
            {
                case VarType.Bit:
                    return varCount + 7 / 8;
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
                case VarType.DateTime:
                    return varCount * 8;
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

                // Fill byte in response when bytecount is odd
                if (dataItem.Count % 2 != 0 && (dataItem.VarType == VarType.Byte || dataItem.VarType == VarType.Bit))
                    offset++;
            }
        }
    }
}
