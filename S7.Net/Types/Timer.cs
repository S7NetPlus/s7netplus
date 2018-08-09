using System;

namespace S7.Net.Types
{
    /// <summary>
    /// Converts the Timer data type to C# data type
    /// </summary>
    public static class Timer
    {
        /// <summary>
        /// Converts the timer bytes to a double
        /// </summary>
        public static double FromByteArray(byte[] bytes)
        {
            double wert = 0;

            wert = ((bytes[0]) & 0x0F) * 100.0;
            wert += ((bytes[1] >> 4) & 0x0F) * 10.0;
            wert += ((bytes[1]) & 0x0F) * 1.0;

            // this value is not used... may for a nother exponation
            //int unknown = (bytes[0] >> 6) & 0x03;

            switch ((bytes[0] >> 4) & 0x03)
            {
                case 0:
                    wert *= 0.01;
                    break;
                case 1:
                    wert *= 0.1;
                    break;
                case 2:
                    wert *= 1.0;
                    break;
                case 3:
                    wert *= 10.0;
                    break;
            }

            return wert;
        }

        /// <summary>
        /// Converts a ushort (UInt16) to an array of bytes formatted as time
        /// </summary>
        public static byte[] ToByteArray(UInt16 value)
        {
            byte[] bytes = new byte[2];
            bytes[1] = (byte)((int)value & 0xFF);
            bytes[0] = (byte)((int)value >> 8 & 0xFF);

            return bytes;
        }

        /// <summary>
        /// Converts an array of ushorts (Uint16) to an array of bytes formatted as time
        /// </summary>
        public static byte[] ToByteArray(UInt16[] value)
        {
            ByteArray arr = new ByteArray();
            foreach (UInt16 val in value)
                arr.Add(ToByteArray(val));
            return arr.Array;
        }

        /// <summary>
        /// Converts an array of bytes formatted as time to an array of doubles
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double[] ToArray(byte[] bytes)
        {
            double[] values = new double[bytes.Length / 2];

            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 2; cnt++)
                values[cnt] = FromByteArray(new byte[] { bytes[counter++], bytes[counter++] });

            return values;
        }
    }
}
