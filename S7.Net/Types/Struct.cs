using System;
using System.Linq;
using System.Reflection;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the method to convert a C# struct to S7 data types
    /// </summary>
    public static class Struct
    {
        /// <summary>
        /// Gets the size of the struct in bytes.
        /// </summary>
        /// <param name="structType">the type of the struct</param>
        /// <returns>the number of bytes</returns>
        public static int GetStructSize(Type structType)
        {
            double numBytes = 0.0;

            var infos = structType
#if NETSTANDARD1_3
                .GetTypeInfo().DeclaredFields;
#else
                .GetFields();
#endif

            foreach (var info in infos)
            {
                switch (info.FieldType.Name)
                {
                    case "Boolean":
                        numBytes += 0.125;
                        break;
                    case "Byte":
                        numBytes = Math.Ceiling(numBytes);
                        numBytes++;
                        break;
                    case "Int16":
                    case "UInt16":
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        numBytes += 2;
                        break;
                    case "Int32":
                    case "UInt32":
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        numBytes += 4;
                        break;
                    case "Single":
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        numBytes += 4;
                        break;
                    case "Double":
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        numBytes += 8;
                        break;
                    case "String":
                        S7StringAttribute? attribute = info.GetCustomAttributes<S7StringAttribute>().SingleOrDefault();
                        if (attribute == default(S7StringAttribute))
                            throw new ArgumentException("Please add S7StringAttribute to the string field");

                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        numBytes += attribute.ReservedLengthInBytes;
                        break;
                    default:
                        numBytes += GetStructSize(info.FieldType);
                        break;
                }
            }
            return (int)numBytes;
        }

        /// <summary>
        /// Creates a struct of a specified type by an array of bytes.
        /// </summary>
        /// <param name="structType">The struct type</param>
        /// <param name="bytes">The array of bytes</param>
        /// <returns>The object depending on the struct type or null if fails(array-length != struct-length</returns>
        public static object? FromBytes(Type structType, byte[] bytes)
        {
            if (bytes == null)
                return null;

            if (bytes.Length != GetStructSize(structType))
                return null;

            // and decode it
            int bytePos = 0;
            int bitPos = 0;
            double numBytes = 0.0;
            object structValue = Activator.CreateInstance(structType) ??
                throw new ArgumentException($"Failed to create an instance of the type {structType}.", nameof(structType));

            var infos = structValue.GetType()
#if NETSTANDARD1_3
                .GetTypeInfo().DeclaredFields;
#else
                .GetFields();
#endif

            foreach (var info in infos)
            {
                switch (info.FieldType.Name)
                {
                    case "Boolean":
                        // get the value
                        bytePos = (int)Math.Floor(numBytes);
                        bitPos = (int)((numBytes - (double)bytePos) / 0.125);
                        if ((bytes[bytePos] & (int)Math.Pow(2, bitPos)) != 0)
                            info.SetValue(structValue, true);
                        else
                            info.SetValue(structValue, false);
                        numBytes += 0.125;
                        break;
                    case "Byte":
                        numBytes = Math.Ceiling(numBytes);
                        info.SetValue(structValue, (byte)(bytes[(int)numBytes]));
                        numBytes++;
                        break;
                    case "Int16":
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        // get the value
                        ushort source = Word.FromBytes(bytes[(int)numBytes + 1], bytes[(int)numBytes]);
                        info.SetValue(structValue, source.ConvertToShort());
                        numBytes += 2;
                        break;
                    case "UInt16":
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        // get the value
                        info.SetValue(structValue, Word.FromBytes(bytes[(int)numBytes + 1],
                                                                          bytes[(int)numBytes]));
                        numBytes += 2;
                        break;
                    case "Int32":
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        // get the value
                        uint sourceUInt = DWord.FromBytes(bytes[(int)numBytes + 3],
                                                                           bytes[(int)numBytes + 2],
                                                                           bytes[(int)numBytes + 1],
                                                                           bytes[(int)numBytes + 0]);
                        info.SetValue(structValue, sourceUInt.ConvertToInt());
                        numBytes += 4;
                        break;
                    case "UInt32":
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        // get the value
                        info.SetValue(structValue, DWord.FromBytes(bytes[(int)numBytes],
                                                                           bytes[(int)numBytes + 1],
                                                                           bytes[(int)numBytes + 2],
                                                                           bytes[(int)numBytes + 3]));
                        numBytes += 4;
                        break;
                    case "Single":
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        // get the value
                        info.SetValue(structValue, Real.FromByteArray(new byte[] { bytes[(int)numBytes],
                                                                           bytes[(int)numBytes + 1],
                                                                           bytes[(int)numBytes + 2],
                                                                           bytes[(int)numBytes + 3] }));
                        numBytes += 4;
                        break;
                    case "Double":
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        // get the value
                        var data = new byte[8];
                        Array.Copy(bytes, (int)numBytes, data, 0, 8);
                        info.SetValue(structValue, LReal.FromByteArray(data));
                        numBytes += 8;
                        break;
                    case "String":
                        S7StringAttribute? attribute = info.GetCustomAttributes<S7StringAttribute>().SingleOrDefault();
                        if (attribute == default(S7StringAttribute))
                            throw new ArgumentException("Please add S7StringAttribute to the string field");

                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;

                        // get the value
                        var sData = new byte[attribute.ReservedLengthInBytes];
                        Array.Copy(bytes, (int)numBytes, sData, 0, sData.Length);
                        switch (attribute.Type)
                        {
                            case S7StringType.S7String:
                                info.SetValue(structValue, S7String.FromByteArray(sData));
                                break;
                            case S7StringType.S7WString:
                                info.SetValue(structValue, S7WString.FromByteArray(sData));
                                break;
                            default:
                                throw new ArgumentException("Please use a valid string type for the S7StringAttribute");
                        }

                        numBytes += sData.Length;
                        break;
                    default:
                        var buffer = new byte[GetStructSize(info.FieldType)];
                        if (buffer.Length == 0)
                            continue;
                        Buffer.BlockCopy(bytes, (int)Math.Ceiling(numBytes), buffer, 0, buffer.Length);
                        info.SetValue(structValue, FromBytes(info.FieldType, buffer));
                        numBytes += buffer.Length;
                        break;
                }
            }
            return structValue;
        }

        /// <summary>
        /// Creates a byte array depending on the struct type.
        /// </summary>
        /// <param name="structValue">The struct object</param>
        /// <returns>A byte array or null if fails.</returns>
        public static byte[] ToBytes(object structValue)
        {
            Type type = structValue.GetType();

            int size = Struct.GetStructSize(type);
            byte[] bytes = new byte[size];
            byte[]? bytes2 = null;

            int bytePos = 0;
            int bitPos = 0;
            double numBytes = 0.0;

            var infos = type
#if NETSTANDARD1_3
                .GetTypeInfo().DeclaredFields;
#else
                .GetFields();
#endif

            foreach (var info in infos)
            {
                static TValue GetValueOrThrow<TValue>(FieldInfo fi, object structValue) where TValue : struct
                {
                    var value = fi.GetValue(structValue) as TValue? ??
                        throw new ArgumentException($"Failed to convert value of field {fi.Name} of {structValue} to type {typeof(TValue)}");

                    return value;
                }

                bytes2 = null;
                switch (info.FieldType.Name)
                {
                    case "Boolean":
                        // get the value
                        bytePos = (int)Math.Floor(numBytes);
                        bitPos = (int)((numBytes - (double)bytePos) / 0.125);
                        if (GetValueOrThrow<bool>(info, structValue))
                            bytes[bytePos] |= (byte)Math.Pow(2, bitPos);            // is true
                        else
                            bytes[bytePos] &= (byte)(~(byte)Math.Pow(2, bitPos));   // is false
                        numBytes += 0.125;
                        break;
                    case "Byte":
                        numBytes = (int)Math.Ceiling(numBytes);
                        bytePos = (int)numBytes;
                        bytes[bytePos] = GetValueOrThrow<byte>(info, structValue);
                        numBytes++;
                        break;
                    case "Int16":
                        bytes2 = Int.ToByteArray(GetValueOrThrow<short>(info, structValue));
                        break;
                    case "UInt16":
                        bytes2 = Word.ToByteArray(GetValueOrThrow<ushort>(info, structValue));
                        break;
                    case "Int32":
                        bytes2 = DInt.ToByteArray(GetValueOrThrow<int>(info, structValue));
                        break;
                    case "UInt32":
                        bytes2 = DWord.ToByteArray(GetValueOrThrow<uint>(info, structValue));
                        break;
                    case "Single":
                        bytes2 = Real.ToByteArray(GetValueOrThrow<float>(info, structValue));
                        break;
                    case "Double":
                        bytes2 = LReal.ToByteArray(GetValueOrThrow<double>(info, structValue));
                        break;
                    case "String":
                        S7StringAttribute? attribute = info.GetCustomAttributes<S7StringAttribute>().SingleOrDefault();
                        if (attribute == default(S7StringAttribute))
                            throw new ArgumentException("Please add S7StringAttribute to the string field");

                        bytes2 = attribute.Type switch
                        {
                            S7StringType.S7String => S7String.ToByteArray((string?)info.GetValue(structValue), attribute.ReservedLength),
                            S7StringType.S7WString => S7WString.ToByteArray((string?)info.GetValue(structValue), attribute.ReservedLength),
                            _ => throw new ArgumentException("Please use a valid string type for the S7StringAttribute")
                        };
                        break;
                }
                if (bytes2 != null)
                {
                    // add them
                    numBytes = Math.Ceiling(numBytes);
                    if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                        numBytes++;
                    bytePos = (int)numBytes;
                    for (int bCnt = 0; bCnt < bytes2.Length; bCnt++)
                        bytes[bytePos + bCnt] = bytes2[bCnt];
                    numBytes += bytes2.Length;
                }
            }
            return bytes;
        }
    }
}
