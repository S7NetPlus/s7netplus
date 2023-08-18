using System;
using System.Globalization;

namespace S7.Net
{
    /// <summary>
    /// Conversion methods to convert from Siemens numeric format to C# and back
    /// </summary>
    public static class Conversion
    {
        /// <summary>
        /// Converts a binary string to Int32 value
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static int BinStringToInt32(this string txt)
        {
            int ret = 0;

            for (int i = 0; i < txt.Length; i++)
            {
                ret = (ret << 1) | ((txt[i] == '1') ? 1 : 0);
            }
            return ret;
        }

        /// <summary>
        /// Converts a binary string to a byte. Can return null.
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static byte? BinStringToByte(this string txt)
        {
            if (txt.Length == 8) return (byte)BinStringToInt32(txt);
            return null;
        }

        /// <summary>
        /// Converts the value to a binary string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ValToBinString(this object value)
        {
            int cnt = 0;
            int cnt2 = 0;
            int x = 0;
            string txt = "";
            long longValue = 0;

            try
            {
                if (value.GetType().Name.IndexOf("[]") < 0)
                {
                    // ist nur ein Wert
                    switch (value.GetType().Name)
                    {
                        case "Byte":
                            x = 7;
                            longValue = (long)((byte)value);
                            break;
                        case "Int16":
                            x = 15;
                            longValue = (long)((Int16)value);
                            break;
                        case "Int32":
                            x = 31;
                            longValue = (long)((Int32)value);
                            break;
                        case "Int64":
                            x = 63;
                            longValue = (long)((Int64)value);
                            break;
                        default:
                            throw new Exception();
                    }

                    for (cnt = x; cnt >= 0; cnt += -1)
                    {
                        if (((Int64)longValue & (Int64)Math.Pow(2, cnt)) > 0)
                            txt += "1";
                        else
                            txt += "0";
                    }
                }
                else
                {
                    // ist ein Array
                    switch (value.GetType().Name)
                    {
                        case "Byte[]":
                            x = 7;
                            byte[] ByteArr = (byte[])value;
                            for (cnt2 = 0; cnt2 <= ByteArr.Length - 1; cnt2++)
                            {
                                for (cnt = x; cnt >= 0; cnt += -1)
                                    if ((ByteArr[cnt2] & (byte)Math.Pow(2, cnt)) > 0) txt += "1"; else txt += "0";
                            }
                            break;
                        case "Int16[]":
                            x = 15;
                            Int16[] Int16Arr = (Int16[])value;
                            for (cnt2 = 0; cnt2 <= Int16Arr.Length - 1; cnt2++)
                            {
                                for (cnt = x; cnt >= 0; cnt += -1)
                                    if ((Int16Arr[cnt2] & (byte)Math.Pow(2, cnt)) > 0) txt += "1"; else txt += "0";
                            }
                            break;
                        case "Int32[]":
                            x = 31;
                            Int32[] Int32Arr = (Int32[])value;
                            for (cnt2 = 0; cnt2 <= Int32Arr.Length - 1; cnt2++)
                            {
                                for (cnt = x; cnt >= 0; cnt += -1)
                                    if ((Int32Arr[cnt2] & (byte)Math.Pow(2, cnt)) > 0) txt += "1"; else txt += "0";
                            }
                            break;
                        case "Int64[]":
                            x = 63;
                            byte[] Int64Arr = (byte[])value;
                            for (cnt2 = 0; cnt2 <= Int64Arr.Length - 1; cnt2++)
                            {
                                for (cnt = x; cnt >= 0; cnt += -1)
                                    if ((Int64Arr[cnt2] & (byte)Math.Pow(2, cnt)) > 0) txt += "1"; else txt += "0";
                            }
                            break;
                        default:
                            throw new Exception();
                    }
                }
                return txt;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Helper to get a bit value given a byte and the bit index.
        /// <br/>
        /// <example>
        ///   Get the bit at DB1.DBX0.5:
        ///   <code>
        ///     byte data = ReadByte("DB1.DBB0");
        ///     bool bit = data.SelectBit(5);
        ///   </code>
        /// </example>
        /// </summary>
        /// <param name="data">The data to get from.</param>
        /// <param name="index">The zero-based index of the bit to get.</param>
        /// <returns>The Boolean value will get.</returns>
        public static bool SelectBit(this byte data, int index)
        {
            int mask = 1 << index;
            int result = data & mask;

            return (result != 0);
        }

        /// <summary>
        /// Helper to set a bit value to the given byte at the bit index.
        /// <br/>
        /// <example>
        ///   Set the bit at index 4:
        ///   <code>
        ///     byte data = 0;
        ///     data.SetBit(4, true);
        ///   </code>
        /// </example>
        /// </summary>
        /// <param name="data">The data to be modified.</param>
        /// <param name="index">The zero-based index of the bit to set.</param>
        /// <param name="value">The Boolean value to assign to the bit.</param>
        public static void SetBit(this ref byte data, int index, bool value)
        {
            if ((uint)index > 7)
            {
                return;
            }

            if (value)
            {
                byte mask = (byte)(1 << index);
                data |= mask;
            }
            else
            {
                byte mask = (byte)~(1 << index);
                data &= mask;
            }
        }

        /// <summary>
        /// Converts from ushort value to short value; it's used to retrieve negative values from words
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static short ConvertToShort(this ushort input)
        {
            short output;
            output = short.Parse(input.ToString("X"), NumberStyles.HexNumber);
            return output;
        }

        /// <summary>
        /// Converts from short value to ushort value; it's used to pass negative values to DWs
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static ushort ConvertToUshort(this short input)
        {
            ushort output;
            output = ushort.Parse(input.ToString("X"), NumberStyles.HexNumber);
            return output;
        }

        /// <summary>
        /// Converts from UInt32 value to Int32 value; it's used to retrieve negative values from DBDs
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Int32 ConvertToInt(this uint input)
        {
            int output;
            output = int.Parse(input.ToString("X"), NumberStyles.HexNumber);
            return output;
        }

        /// <summary>
        /// Converts from Int32 value to UInt32 value; it's used to pass negative values to DBDs
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static UInt32 ConvertToUInt(this int input)
        {
            uint output;
            output = uint.Parse(input.ToString("X"), NumberStyles.HexNumber);
            return output;
        }

        /// <summary>
        /// Converts from float to DWord (DBD)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static UInt32 ConvertToUInt(this float input)
        {
            uint output;
            output = S7.Net.Types.DWord.FromByteArray(S7.Net.Types.Real.ToByteArray(input));
            return output;
        }

        /// <summary>
        /// Converts from DWord (DBD) to float
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static float ConvertToFloat(this uint input)
        {
            float output;
            output = S7.Net.Types.Real.FromByteArray(S7.Net.Types.DWord.ToByteArray(input));
            return output;
        }
    }
}
