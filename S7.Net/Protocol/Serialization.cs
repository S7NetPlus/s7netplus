using System;
using System.Collections.Generic;
using S7.Net.Types;

namespace S7.Net.Protocol
{
    internal static class Serialization
    {
        public static ushort GetWordAt(IList<byte> buf, int index)
        {
            return (ushort)((buf[index] << 8) + buf[index]);
        }

        public static byte[] SerializeDataItem(DataItem dataItem)
        {
            if (dataItem.Value == null)
            {
                throw new Exception($"DataItem.Value is null, cannot serialize. StartAddr={dataItem.StartByteAdr} VarType={dataItem.VarType}");
            }
            if (dataItem.Value is string s)
                return dataItem.VarType == VarType.S7String
                    ? S7String.ToByteArray(s, dataItem.Count)
                    : Types.String.ToByteArray(s, dataItem.Count);

            return SerializeValue(dataItem.Value);
        }

        public static byte[] SerializeValue(object value)
        {
            switch (value.GetType().Name)
            {
                case "Boolean":
                    return new[] { (byte)((bool)value ? 1 : 0) };
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
                case "Single":
                    return Types.Real.ToByteArray((float)value);
                case "Double":
                    return Types.LReal.ToByteArray((double)value);
                case "DateTime":
                    return Types.DateTime.ToByteArray((System.DateTime) value);
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
                case "Single[]":
                    return Types.Real.ToByteArray((float[])value);
                case "Double[]":
                    return Types.LReal.ToByteArray((double[])value);
                case "String":
                    // Hack: This is backwards compatible with the old code, but functionally it's broken
                    // if the consumer does not pay attention to string length.
                    var stringVal = (string) value;
                    return Types.String.ToByteArray(stringVal, stringVal.Length);
                case "DateTime[]":
                    return Types.DateTime.ToByteArray((System.DateTime[]) value);
                case "DateTimeLong[]":
                    return Types.DateTimeLong.ToByteArray((System.DateTime[])value);
                default:
                    throw new InvalidVariableTypeException();
            }
        }

        public static void SetAddressAt(ByteArray buffer, int index, int startByte, byte bitNumber)
        {
            var start = startByte * 8 + bitNumber;
            buffer[index + 2] = (byte)start;
            start >>= 8;
            buffer[index + 1] = (byte)start;
            start >>= 8;
            buffer[index] = (byte)start;
        }

        public static void SetWordAt(ByteArray buffer, int index, ushort value)
        {
            buffer[index] = (byte)(value >> 8);
            buffer[index + 1] = (byte)value;
        }
    }
}
