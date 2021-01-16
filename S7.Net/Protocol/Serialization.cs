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
            if (dataItem.Value is null)
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
            return (value.GetType().Name) switch
            {
                "Boolean"  => new[] { (byte)((bool)value ? 1 : 0) },
                "Byte"     => Types.Byte.ToByteArray((byte)value),
                "Int16"    => Types.Int.ToByteArray((Int16)value),
                "UInt16"   => Types.Word.ToByteArray((UInt16)value),
                "Int32"    => Types.DInt.ToByteArray((Int32)value),
                "UInt32"   => Types.DWord.ToByteArray((UInt32)value),
                "Single"   => Types.Real.ToByteArray((float)value),
                "Double"   => Types.LReal.ToByteArray((double)value),
                "DateTime" => Types.DateTime.ToByteArray((System.DateTime)value),
                "Byte[]"   => (byte[])value,
                "Int16[]"  => Types.Int.ToByteArray((Int16[])value),
                "UInt16[]" => Types.Word.ToByteArray((UInt16[])value),
                "Int32[]"  => Types.DInt.ToByteArray((Int32[])value),
                "UInt32[]" => Types.DWord.ToByteArray((UInt32[])value),
                "Single[]" => Types.Real.ToByteArray((float[])value),
                "Double[]" => Types.LReal.ToByteArray((double[])value),
                "String"   =>
                // Hack: This is backwards compatible with the old code, but functionally it's broken
                // if the consumer does not pay attention to string length.
                Types.String.ToByteArray((string)value, ((string)value).Length),
                "DateTime[]"     => Types.DateTime.ToByteArray((System.DateTime[])value),
                "DateTimeLong[]" => Types.DateTimeLong.ToByteArray((System.DateTime[])value),
                _                => throw new InvalidVariableTypeException(),
            };
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
