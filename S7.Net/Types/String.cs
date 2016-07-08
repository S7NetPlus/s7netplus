namespace S7.Net.Types
{
    public static class String
    {
        // publics
        #region ToByteArray
        public static byte[] ToByteArray(string value)
        {
            string txt = (string)value;
            char[] ca = txt.ToCharArray();
            byte[] bytes = new byte[txt.Length];
            for (int cnt = 0; cnt <= ca.Length - 1; cnt++)
                bytes[cnt] = (byte)Asc(ca[cnt].ToString());
            return bytes;
        }
        #endregion

        #region FromByteArray
        public static string FromByteArray(byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString(bytes);
        }
        #endregion

        // privates
        private static int Asc(string s)
        {
            byte[] b = System.Text.Encoding.ASCII.GetBytes(s);
            if (b.Length > 0)
                return b[0];
            return 0;
        }
    }
}
