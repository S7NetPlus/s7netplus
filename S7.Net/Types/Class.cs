using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the methods to convert a C# class to S7 data types
    /// </summary>
    public static class Class
    {
        private static IEnumerable<PropertyInfo> GetAccessableProperties(Type classType)
        {
            return classType
#if NETSTANDARD1_3
                .GetTypeInfo().DeclaredProperties.Where(p => p.SetMethod != null);
#else
                .GetProperties(
                    BindingFlags.SetProperty |
                    BindingFlags.Public |
                    BindingFlags.Instance)
                .Where(p => p.GetSetMethod() != null);
#endif

        }

        private static double GetIncreasedNumberOfBytes(double numBytes, Type type, CpuType cpu)
        {
            switch (type.Name)
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

                case "DateTime":
                    numBytes = Math.Ceiling(numBytes);
                    if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                        numBytes++;
                    // https://support.industry.siemens.com/cs/document/43566349/in-step-7-(tia-portal)-how-can-you-input-read-out-and-edit-the-date-and-time-for-the-cpu-modules-?dti=0&lc=en-WW
                    // Per Siemens documentation, DateTime structures are model specific, and compatibility to exchange types
                    // is not supported by Siemens.
                    
#if NETSTANDARD1_3
                    S7DateTimeAttribute dateAttribute = type.GetTypeInfo().GetCustomAttributes<S7DateTimeAttribute>().SingleOrDefault() ??
                                                     (cpu switch
                    {
                        CpuType.S71200 => new S7DateTimeAttribute(S7DateTimeType.DTL),
                        CpuType.S71500 => new S7DateTimeAttribute(S7DateTimeType.DTL),
                        _ => new S7DateTimeAttribute(S7DateTimeType.DT),
                    });
#else
                    S7DateTimeAttribute dateAttribute = type.GetCustomAttributes<S7DateTimeAttribute>().SingleOrDefault() ??
                                                     (cpu switch
                    {
                        CpuType.S71200 => new S7DateTimeAttribute(S7DateTimeType.DTL),
                        CpuType.S71500 => new S7DateTimeAttribute(S7DateTimeType.DTL),
                        _ => new S7DateTimeAttribute(S7DateTimeType.DT),
                    });

#endif
                    numBytes = dateAttribute.ByteLength;
                    break;

                case "String":
                    numBytes = Math.Ceiling(numBytes);
                    if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                        numBytes++;
                    
#if NETSTANDARD1_3
                    S7StringAttribute? attribute = type.GetTypeInfo().GetCustomAttributes<S7StringAttribute>().SingleOrDefault();
#else
                    S7StringAttribute? attribute = type.GetCustomAttributes<S7StringAttribute>().SingleOrDefault();
#endif
                    if (attribute == default(S7StringAttribute))
                        throw new ArgumentException("Please add S7StringAttribute to the string property");
                    
                    numBytes += attribute.ReservedLengthInBytes;
                    break;
                default:
                    var propertyClass = Activator.CreateInstance(type);
                    numBytes = GetClassSize(propertyClass, numBytes, true, cpu);
                    break;
            }

            return numBytes;
        }

        /// <summary>
        /// Gets the size of the class in bytes.
        /// </summary>
        /// <param name="instance">An instance of the class</param>
        /// <returns>the number of bytes</returns>
        public static double GetClassSize(object instance, double numBytes = 0.0, bool isInnerProperty = false, CpuType cpu = CpuType.S7300)
        {
            var properties = GetAccessableProperties(instance.GetType());
            foreach (var property in properties)
            {
                if (property.PropertyType.IsArray)
                {
                    Type elementType = property.PropertyType.GetElementType();
                    Array array = (Array)property.GetValue(instance, null);
                    if (array.Length <= 0)
                    {
                        throw new Exception("Cannot determine size of class, because an array is defined which has no fixed size greater than zero.");
                    }

                    IncrementToEven(ref numBytes);
                    for (int i = 0; i < array.Length; i++)
                    {
                        numBytes = GetIncreasedNumberOfBytes(numBytes, elementType, cpu);
                    }
                }
                else
                {
                    numBytes = GetIncreasedNumberOfBytes(numBytes, property.PropertyType, cpu);
                }
            }
            if (false == isInnerProperty)
            {
                // enlarge numBytes to next even number because S7-Structs in a DB always will be resized to an even byte count
                numBytes = Math.Ceiling(numBytes);
                if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                    numBytes++;
            }
            return numBytes;
        }

        private static object? GetPropertyValue(Type propertyType, byte[] bytes, ref double numBytes, CpuType cpu)
        {
            object? value = null;

            switch (propertyType.Name)
            {
                case "Boolean":
                    // get the value
                    int bytePos = (int)Math.Floor(numBytes);
                    int bitPos = (int)((numBytes - (double)bytePos) / 0.125);
                    if ((bytes[bytePos] & (int)Math.Pow(2, bitPos)) != 0)
                        value = true;
                    else
                        value = false;
                    numBytes += 0.125;
                    break;
                case "Byte":
                    numBytes = Math.Ceiling(numBytes);
                    value = (byte)(bytes[(int)numBytes]);
                    numBytes++;
                    break;
                case "Int16":
                    numBytes = Math.Ceiling(numBytes);
                    if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                        numBytes++;
                    // hier auswerten
                    ushort source = Word.FromBytes(bytes[(int)numBytes + 1], bytes[(int)numBytes]);
                    value = source.ConvertToShort();
                    numBytes += 2;
                    break;
                case "UInt16":
                    numBytes = Math.Ceiling(numBytes);
                    if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                        numBytes++;
                    // hier auswerten
                    value = Word.FromBytes(bytes[(int)numBytes + 1], bytes[(int)numBytes]);
                    numBytes += 2;
                    break;
                case "Int32":
                    numBytes = Math.Ceiling(numBytes);
                    if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                        numBytes++;
                    // hier auswerten
                    uint sourceUInt = DWord.FromBytes(bytes[(int)numBytes + 3],
                                                                       bytes[(int)numBytes + 2],
                                                                       bytes[(int)numBytes + 1],
                                                                       bytes[(int)numBytes + 0]);
                    value = sourceUInt.ConvertToInt();
                    numBytes += 4;
                    break;
                case "UInt32":
                    numBytes = Math.Ceiling(numBytes);
                    if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                        numBytes++;
                    // hier auswerten
                    value = DWord.FromBytes(
                        bytes[(int)numBytes],
                        bytes[(int)numBytes + 1],
                        bytes[(int)numBytes + 2],
                        bytes[(int)numBytes + 3]);
                    numBytes += 4;
                    break;
                case "Single":
                    numBytes = Math.Ceiling(numBytes);
                    if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                        numBytes++;
                    // hier auswerten
                    value = Real.FromByteArray(
                        new byte[] {
                            bytes[(int)numBytes],
                            bytes[(int)numBytes + 1],
                            bytes[(int)numBytes + 2],
                            bytes[(int)numBytes + 3] });
                    numBytes += 4;
                    break;
                case "Double":
                    numBytes = Math.Ceiling(numBytes);
                    if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                        numBytes++;
                    var buffer = new byte[8];
                    Array.Copy(bytes, (int)numBytes, buffer, 0, 8);
                    // hier auswerten
                    value = LReal.FromByteArray(buffer);
                    numBytes += 8;
                    break;
                case "DateTime":
                    numBytes = Math.Ceiling(numBytes);
                    if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                        numBytes++;
                    // https://support.industry.siemens.com/cs/document/43566349/in-step-7-(tia-portal)-how-can-you-input-read-out-and-edit-the-date-and-time-for-the-cpu-modules-?dti=0&lc=en-WW
                    // Per Siemens documentation, DateTime structures are model specific, and compatibility to exchange types
                    // is not supported by Siemens.

                    // If the property does not have a S7DateTimeAttribute set, then set a default attribute based on what
                    // the CPU's default DateTime parsing mechanism is
#if NETSTANDARD1_3
                    S7DateTimeAttribute dateAttribute =
                        propertyType.GetTypeInfo().GetCustomAttributes<S7DateTimeAttribute>().SingleOrDefault() ??
                        cpu switch
                        {
                            CpuType.S71200 => new S7DateTimeAttribute(S7DateTimeType.DTL),
                            CpuType.S71500 => new S7DateTimeAttribute(S7DateTimeType.DTL),
                            _ => new S7DateTimeAttribute(S7DateTimeType.DT),
                        };
#else
                    S7DateTimeAttribute dateAttribute =
                        propertyType.GetCustomAttributes<S7DateTimeAttribute>().SingleOrDefault() ??
                        cpu switch
                        {
                            CpuType.S71200 => new S7DateTimeAttribute(S7DateTimeType.DTL),
                            CpuType.S71500 => new S7DateTimeAttribute(S7DateTimeType.DTL),
                            _ => new S7DateTimeAttribute(S7DateTimeType.DT),
                        };

#endif
                    
                    switch (cpu)
                    {
                        case CpuType.S71500:
                            value = dateAttribute.Type switch
                            {
                                S7DateTimeType.DTL => DateTimeLong.FromByteArray(new byte[]
                                {
                                    bytes[(int)numBytes], bytes[(int)numBytes + 1], bytes[(int)numBytes + 2],
                                    bytes[(int)numBytes + 3], bytes[(int)numBytes + 4], bytes[(int)numBytes + 5],
                                    bytes[(int)numBytes + 6], bytes[(int)numBytes + 7], bytes[(int)numBytes + 8],
                                    bytes[(int)numBytes + 9], bytes[(int)numBytes + 10], bytes[(int)numBytes + 11],
                                }),
                                _ => DateTime.FromByteArray(new byte[]
                                {
                                    bytes[(int)numBytes], bytes[(int)numBytes + 1], bytes[(int)numBytes + 2],
                                    bytes[(int)numBytes + 3], bytes[(int)numBytes + 4], bytes[(int)numBytes + 5],
                                    bytes[(int)numBytes + 6], bytes[(int)numBytes + 7],
                                })
                            };
                            numBytes += dateAttribute.ByteLength;
                            break;
                        case CpuType.S71200:
                            value = DateTimeLong.FromByteArray(new byte[]
                            {
                                bytes[(int)numBytes], bytes[(int)numBytes + 1], bytes[(int)numBytes + 2],
                                bytes[(int)numBytes + 3], bytes[(int)numBytes + 4], bytes[(int)numBytes + 5],
                                bytes[(int)numBytes + 6], bytes[(int)numBytes + 7], bytes[(int)numBytes + 8],
                                bytes[(int)numBytes + 9], bytes[(int)numBytes + 10], bytes[(int)numBytes + 11],
                            });
                            numBytes += 12;
                            break;
                        default:
                            value = DateTime.FromByteArray(new byte[]
                            {
                                bytes[(int)numBytes], bytes[(int)numBytes + 1], bytes[(int)numBytes + 2],
                                bytes[(int)numBytes + 3], bytes[(int)numBytes + 4], bytes[(int)numBytes + 5],
                                bytes[(int)numBytes + 6], bytes[(int)numBytes + 7],
                            });
                            numBytes += 8;
                            break;
                    }

                    break;
                case "String":
                    numBytes = Math.Ceiling(numBytes);
                    if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                        numBytes++;
#if NETSTANDARD1_3
                    S7StringAttribute? attribute = propertyType.GetTypeInfo().GetCustomAttributes<S7StringAttribute>().SingleOrDefault();
#else
                    S7StringAttribute? attribute = propertyType.GetCustomAttributes<S7StringAttribute>().SingleOrDefault();
#endif
                    if (attribute == default(S7StringAttribute))
                        throw new ArgumentException("Please add S7StringAttribute to the string property");

                    var parsebytes = new byte[attribute.ReservedLength];
                    Array.Copy(bytes, (int)numBytes, parsebytes, 0, attribute.ReservedLength);
                    value = attribute.Type switch
                    {
                        S7StringType.S7String => S7String.FromByteArray(parsebytes),
                        S7StringType.S7WString => S7WString.FromByteArray(parsebytes),
                        _ => throw new ArgumentException("Please use a valid string type for the S7StringAttribute")
                    };
                    break;
                default:
                    var propClass = Activator.CreateInstance(propertyType);
                    numBytes = FromBytes(propClass, bytes, numBytes, cpu: cpu);
                    value = propClass;
                    break;
            }

            return value;
        }

        /// <summary>
        /// Sets the object's values with the given array of bytes
        /// </summary>
        /// <param name="sourceClass">The object to fill in the given array of bytes</param>
        /// <param name="bytes">The array of bytes</param>
        /// <param name="numBytes"></param>
        /// <param name="isInnerClass"></param>
        /// <param name="cpu"></param>
        public static double FromBytes(object sourceClass, byte[] bytes, double numBytes = 0, bool isInnerClass = false, CpuType cpu = CpuType.S7300)
        {
            if (bytes == null)
                return numBytes;

            var properties = GetAccessableProperties(sourceClass.GetType());
            foreach (var property in properties)
            {
                if (property.PropertyType.IsArray)
                {
                    Array array = (Array)property.GetValue(sourceClass, null);
                    IncrementToEven(ref numBytes);
                    Type elementType = property.PropertyType.GetElementType();
                    for (int i = 0; i < array.Length && numBytes < bytes.Length; i++)
                    {
                        array.SetValue(
                            GetPropertyValue(elementType, bytes, ref numBytes, cpu),
                            i);
                    }
                }
                else
                {
                    property.SetValue(
                        sourceClass,
                        GetPropertyValue(property.PropertyType, bytes, ref numBytes, cpu),
                        null);
                }
            }

            return numBytes;
        }

        private static double SetBytesFromProperty(object propertyValue, byte[] bytes, double numBytes, CpuType cpu)
        {
            int bytePos = 0;
            int bitPos = 0;
            byte[]? bytes2 = null;

            switch (propertyValue.GetType().Name)
            {
                case "Boolean":
                    // get the value
                    bytePos = (int)Math.Floor(numBytes);
                    bitPos = (int)((numBytes - (double)bytePos) / 0.125);
                    if ((bool)propertyValue)
                        bytes[bytePos] |= (byte)Math.Pow(2, bitPos);            // is true
                    else
                        bytes[bytePos] &= (byte)(~(byte)Math.Pow(2, bitPos));   // is false
                    numBytes += 0.125;
                    break;
                case "Byte":
                    numBytes = (int)Math.Ceiling(numBytes);
                    bytePos = (int)numBytes;
                    bytes[bytePos] = (byte)propertyValue;
                    numBytes++;
                    break;
                case "Int16":
                    bytes2 = Int.ToByteArray((Int16)propertyValue);
                    break;
                case "UInt16":
                    bytes2 = Word.ToByteArray((UInt16)propertyValue);
                    break;
                case "Int32":
                    bytes2 = DInt.ToByteArray((Int32)propertyValue);
                    break;
                case "UInt32":
                    bytes2 = DWord.ToByteArray((UInt32)propertyValue);
                    break;
                case "Single":
                    bytes2 = Real.ToByteArray((float)propertyValue);
                    break;
                case "Double":
                    bytes2 = LReal.ToByteArray((double)propertyValue);
                    break;
                case "DateTime":

#if NETSTANDARD1_3
                    S7DateTimeAttribute dateAttribute = propertyValue.GetType().GetTypeInfo().GetCustomAttributes<S7DateTimeAttribute>().SingleOrDefault() ??
                                                     (cpu switch
                    {
                        CpuType.S71200 => new S7DateTimeAttribute(S7DateTimeType.DTL),
                        CpuType.S71500 => new S7DateTimeAttribute(S7DateTimeType.DTL),
                        _ => new S7DateTimeAttribute(S7DateTimeType.DT),
                    });
#else
                    S7DateTimeAttribute dateAttribute = propertyValue.GetType().GetCustomAttributes<S7DateTimeAttribute>().SingleOrDefault() ??
                                                        (cpu switch
                                                        {
                                                            CpuType.S71200 => new S7DateTimeAttribute(S7DateTimeType.DTL),
                                                            CpuType.S71500 => new S7DateTimeAttribute(S7DateTimeType.DTL),
                                                            _ => new S7DateTimeAttribute(S7DateTimeType.DT),
                                                        });

#endif
                    numBytes = dateAttribute.ByteLength;

                    bytes2 = dateAttribute.Type switch
                    {
                        S7DateTimeType.DTL => DateTimeLong.ToByteArray((System.DateTime)propertyValue),
                        _ => DateTime.ToByteArray((System.DateTime)propertyValue)
                    };

                    break;
                case "String":
#if NETSTANDARD1_3
                    S7StringAttribute? attribute = propertyValue.GetType().GetTypeInfo().GetCustomAttributes<S7StringAttribute>().SingleOrDefault();
#else
                    S7StringAttribute? attribute = propertyValue.GetType().GetCustomAttributes<S7StringAttribute>().SingleOrDefault();
#endif
                    if (attribute == default(S7StringAttribute))
                        throw new ArgumentException("Please add S7StringAttribute to the string property");

                    bytes2 = attribute.Type switch
                    {
                        S7StringType.S7String => S7String.ToByteArray((string)propertyValue, attribute.ReservedLength),
                        S7StringType.S7WString => S7WString.ToByteArray((string)propertyValue, attribute.ReservedLength),
                        _ => throw new ArgumentException("Please use a valid string type for the S7StringAttribute")
                    };
                    break;
                default:
                    numBytes = ToBytes(propertyValue, bytes, numBytes, cpu: cpu);
                    break;
            }

            if (bytes2 != null)
            {
                IncrementToEven(ref numBytes);

                bytePos = (int)numBytes;
                for (int bCnt = 0; bCnt < bytes2.Length; bCnt++)
                    bytes[bytePos + bCnt] = bytes2[bCnt];
                numBytes += bytes2.Length;
            }

            return numBytes;
        }

        /// <summary>
        /// Creates a byte array depending on the struct type.
        /// </summary>
        /// <param name="sourceClass">The struct object</param>
        /// <returns>A byte array or null if fails.</returns>
        public static double ToBytes(object sourceClass, byte[] bytes, double numBytes = 0.0, CpuType cpu = CpuType.S7300)
        {
            var properties = GetAccessableProperties(sourceClass.GetType());
            foreach (var property in properties)
            {
                if (property.PropertyType.IsArray)
                {
                    Array array = (Array)property.GetValue(sourceClass, null);
                    IncrementToEven(ref numBytes);
                    Type elementType = property.PropertyType.GetElementType();
                    for (int i = 0; i < array.Length && numBytes < bytes.Length; i++)
                    {
                        numBytes = SetBytesFromProperty(array.GetValue(i), bytes, numBytes, cpu);
                    }
                }
                else
                {
                    numBytes = SetBytesFromProperty(property.GetValue(sourceClass, null), bytes, numBytes, cpu);
                }
            }
            return numBytes;
        }

        private static void IncrementToEven(ref double numBytes)
        {
            numBytes = Math.Ceiling(numBytes);
            if (numBytes % 2 > 0) numBytes++;
        }
    }
}
