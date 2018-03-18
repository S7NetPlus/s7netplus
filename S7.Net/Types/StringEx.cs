namespace S7.Net.Types
{
    /// <summary>
    /// Contains the methods to convert from S7 strings to C# strings
    ///   there are two kinds how strings a send. This one is with a pre of two bytes
    ///   they contain the length of the string
    /// </summary>
    public static class StringEx
    {
        /// <summary>
        /// Converts S7 bytes to a string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string FromByteArray(byte[] bytes)
        {
            if (bytes.Length < 2) return "";

            int size = bytes[0];
            int length = bytes[1];

            return System.Text.Encoding.ASCII.GetString(bytes, 2, length);
        }
        
    }
}
