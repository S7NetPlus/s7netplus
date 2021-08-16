using System;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the conversion methods to convert Real from S7 plc to C# float.
    /// </summary>
    [Obsolete("Class Single is obsolete. Use Real instead.")]
    public static class Single
    {
        /// <summary>
        /// Converts a S7 Real (4 bytes) to float
        /// </summary>
        public static float FromByteArray(byte[] bytes) => Real.FromByteArray(bytes);

        /// <summary>
        /// Converts a S7 DInt to float
        /// </summary>
        public static float FromDWord(Int32 value)
        {
            byte[] b = DInt.ToByteArray(value);
            float d = FromByteArray(b);
            return d;
        }

        /// <summary>
        /// Converts a S7 DWord to float
        /// </summary>
        public static float FromDWord(UInt32 value)
        {
            byte[] b = DWord.ToByteArray(value);
            float d = FromByteArray(b);
            return d;
        }


        /// <summary>
        /// Converts a double to S7 Real (4 bytes)
        /// </summary>
        public static byte[] ToByteArray(float value) => Real.ToByteArray(value);

        /// <summary>
        /// Converts an array of float to an array of bytes 
        /// </summary>
        public static byte[] ToByteArray(float[] value)
        {
            ByteArray arr = new ByteArray();
            foreach (float val in value)
                arr.Add(ToByteArray(val));
            return arr.Array;
        }

        /// <summary>
        /// Converts an array of S7 Real to an array of float
        /// </summary>
        public static float[] ToArray(byte[] bytes)
        {
            float[] values = new float[bytes.Length / 4];

            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 4; cnt++)
                values[cnt] = FromByteArray(new byte[] { bytes[counter++], bytes[counter++], bytes[counter++], bytes[counter++] });

            return values;
        }
        
    }
}
