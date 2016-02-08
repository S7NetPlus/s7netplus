using System;

namespace S7.Net.Types
{
    public static class DInt
    {
        // publics
        #region FromByteArray
        public static Int32 FromByteArray(byte[] bytes)
        {
            return FromBytes(bytes[3], bytes[2], bytes[1], bytes[0]);
        }
        #endregion
        #region FromBytes
        public static Int32 FromBytes(byte v1, byte v2, byte v3, byte v4)
        {
            return (Int32)(v1 + v2 * Math.Pow(2, 8) + v3 * Math.Pow(2, 16) + v4 * Math.Pow(2, 24));
        }
        #endregion

        #region ToByteArray
        public static byte[] ToByteArray(Int32 value)
        {
            byte[] bytes = new byte[4];
            int x = 4;
            long valLong = (long)((Int32)value);
            for (int cnt = 0; cnt < x; cnt++)
            {
                Int64 x1 = (Int64)Math.Pow(256, (cnt));

                Int64 x3 = (Int64)(valLong / x1);
                bytes[x - cnt - 1] = (byte)(x3 & 255);
                valLong -= bytes[x - cnt - 1] * x1;
            }
            return bytes;
        }

        public static byte[] ToByteArray(Int32[] value)
        {
            ByteArray arr = new ByteArray();
            foreach (Int32 val in value)
                arr.Add(ToByteArray(val));
            return arr.array;
        }
        #endregion
        #region ToArray
        public static Int32[] ToArray(byte[] bytes)
        {
            Int32[] values = new Int32[bytes.Length / 4];

            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 4; cnt++)
                values[cnt] = FromByteArray(new byte[] { bytes[counter++], bytes[counter++], bytes[counter++], bytes[counter++] });

            return values;
        }
        #endregion

        // conversion
        public static Int32 CDWord(Int64 value)
        {
            if (value > Int32.MaxValue)
            {
                value -= (long)Int32.MaxValue + 1;
                value = (long)Int32.MaxValue + 1 - value;
                value *= -1;
            }
            return (int)value;
        }
    }
}
