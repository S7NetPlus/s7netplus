namespace S7.Net.Types
{
    /// <summary>
    /// Contains the methods to convert from S7 Array of Chars (like a const char[N] C-String) to C# strings
    /// </summary>
    public class String
    {
        /// <summary>
        /// Converts a string to <paramref name="reservedLength"/> of bytes, padded with 0-bytes if required.
        /// </summary>
        /// <param name="value">The string to write to the PLC.</param>
        /// <param name="reservedLength">The amount of bytes reserved for the <paramref name="value"/> in the PLC.</param>
        public static byte[] ToByteArray(string value, int reservedLength)
        {
            var length = value?.Length;
            if (length > reservedLength) length = reservedLength;
            var bytes = new byte[reservedLength];

            if (length == null || length == 0) return bytes;

            System.Text.Encoding.ASCII.GetBytes(value, 0, length.Value, bytes, 0);

            return bytes;
        }
        
        /// <summary>
        /// Converts S7 bytes to a string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string FromByteArray(byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

    }
}
