using System;
using System.Collections.Generic;
using S7.Net.Types;

namespace S7.Net.Protocol
{
    internal static class Serialization
    {
        public static ushort GetWordAt(IList<byte> buf, int index)
        {
            return (ushort) ((buf[index] << 8) + buf[index]);
        }

        public static byte[] SerializeValue(object value)
        {
            switch (value.GetType().Name)
            {
                case "Byte":
                    return Types.Byte.ToByteArray((byte) value);
                case "Int16":
                    return Types.Int.ToByteArray((Int16) value);
                case "UInt16":
                    return Types.Word.ToByteArray((UInt16) value);
                case "Int32":
                    return Types.DInt.ToByteArray((Int32) value);
                case "UInt32":
                    return Types.DWord.ToByteArray((UInt32) value);
                case "Double":
                    return Types.Double.ToByteArray((double) value);
                case "Byte[]":
                    return (byte[]) value;
                case "Int16[]":
                    return Types.Int.ToByteArray((Int16[]) value);
                case "UInt16[]":
                    return Types.Word.ToByteArray((UInt16[]) value);
                case "Int32[]":
                    return Types.DInt.ToByteArray((Int32[]) value);
                case "UInt32[]":
                    return Types.DWord.ToByteArray((UInt32[]) value);
                case "Double[]":
                    return Types.Double.ToByteArray((double[]) value);
                case "String":
                    return Types.String.ToByteArray(value as string);
                default:
                    throw new InvalidVariableTypeException();
            }
        }

        public static void SetAddressAt(ByteArray buffer, int index, int startByte, byte bitNumber)
        {
            var start = startByte * 8 + bitNumber;
            buffer[index + 2] = (byte) start;
            start = start >> 8;
            buffer[index + 1] = (byte) start;
            start = start >> 8;
            buffer[index] = (byte) start;
        }

        public static void SetWordAt(ByteArray buffer, int index, ushort value)
        {
            buffer[index] = (byte) (value >> 8);
            buffer[index + 1] = (byte) value;
        }
    }
}
