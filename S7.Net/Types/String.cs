namespace S7.Net.Types
{
    /// <summary>
    /// Contains the methods to convert from S7 strings to C# strings
    /// </summary>
    public static class String
    {
        /// <summary>
        /// Converts a string to S7 bytes
        /// </summary>
        public static byte[] ToByteArray(string value)
        {
            string txt = (string)value;
            char[] ca = txt.ToCharArray();
            byte[] bytes = new byte[txt.Length];
            for (int cnt = 0; cnt <= ca.Length - 1; cnt++)
                bytes[cnt] = (byte)Asc(ca[cnt].ToString());
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
        
        private static int Asc(string s)
        {
            byte[] b = System.Text.Encoding.ASCII.GetBytes(s);
            if (b.Length > 0)
                return b[0];
            return 0;
        }
    }
}
