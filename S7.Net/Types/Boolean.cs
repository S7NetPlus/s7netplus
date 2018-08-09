namespace S7.Net.Types
{
    /// <summary>
    /// Contains the methods to read, set and reset bits inside bytes
    /// </summary>
    public static class Boolean
    {
        /// <summary>
        /// Returns the value of a bit in a bit, given the address of the bit
        /// </summary>
        public static bool GetValue(byte value, int bit)
        {
            return (((int)value & (1 << bit)) != 0);
        }

        /// <summary>
        /// Sets the value of a bit to 1 (true), given the address of the bit
        /// </summary>
        public static byte SetBit(byte value, int bit)
        {
            return (byte)((value | (1 << bit)) & 0xFF);
        }

        /// <summary>
        /// Resets the value of a bit to 0 (false), given the address of the bit
        /// </summary>
        public static byte ClearBit(byte value, int bit)
        {
            return (byte)((value | (~(1 << bit))) & 0xFF);
        }

    }
}
