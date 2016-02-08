using System;

namespace S7.Net.Types
{
    public static class Int
    {
        // publics
        #region FromByteArray
        public static Int16 FromByteArray(byte[] bytes)
        {
            // bytes[0] -> HighByte
            // bytes[1] -> LowByte
            return FromBytes(bytes[1], bytes[0]);
        }
        #endregion
        #region FromBytes
        public static Int16 FromBytes(byte LoVal, byte HiVal)
        {
            return (Int16)(HiVal * 256 + LoVal);
        }
        #endregion

        #region ToByteArray
        public static byte[] ToByteArray(Int16 value)
        {
            byte[] bytes = new byte[2];
            int x = 2;
            long valLong = (long)((Int16)value);
            for (int cnt = 0; cnt < x; cnt++)
            {
                Int64 x1 = (Int64)Math.Pow(256, (cnt));

                Int64 x3 = (Int64)(valLong / x1);
                bytes[x - cnt - 1] = (byte)(x3 & 255);
                valLong -= bytes[x - cnt - 1] * x1;
            }
            return bytes;
        }

        public static byte[] ToByteArray(Int16[] value)
        {
            ByteArray arr = new ByteArray();
            foreach (Int16 val in value)
                arr.Add(ToByteArray(val));
            return arr.array;
        }
        #endregion
        #region ToArray
        public static Int16[] ToArray(byte[] bytes)
        {
            Int16[] values = new Int16[bytes.Length / 2];

            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 2; cnt++)
                values[cnt] = FromByteArray(new byte[] { bytes[counter++], bytes[counter++] });

            return values;
        }
        #endregion

        // conversion
        public static Int16 CWord(int value)
        {
            if (value > 32767)
            {
                value -= 32768;
                value = 32768 - value;
                value *= -1;
            }
            return (short)value;
        }

    }
}
