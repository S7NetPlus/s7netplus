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
        /// Sets the value of a bit to 1 (true), given the address of the bit. Returns
        /// a copy of the value with the bit set.
        /// </summary>
        /// <param name="value">The input value to modify.</param>
        /// <param name="bit">The index (zero based) of the bit to set.</param>
        /// <returns>The modified value with the bit at index set.</returns>
        public static byte SetBit(byte value, int bit)
        {
            SetBit(ref value, bit);

            return value;
        }

        /// <summary>
        /// Sets the value of a bit to 1 (true), given the address of the bit.
        /// </summary>
        /// <param name="value">The value to modify.</param>
        /// <param name="bit">The index (zero based) of the bit to set.</param>
        public static void SetBit(ref byte value, int bit)
        {
            value = (byte) ((value | (1 << bit)) & 0xFF);
        }

        /// <summary>
        /// Resets the value of a bit to 0 (false), given the address of the bit. Returns
        /// a copy of the value with the bit cleared.
        /// </summary>
        /// <param name="value">The input value to modify.</param>
        /// <param name="bit">The index (zero based) of the bit to clear.</param>
        /// <returns>The modified value with the bit at index cleared.</returns>
        public static byte ClearBit(byte value, int bit)
        {
            ClearBit(ref value, bit);

            return value;
        }

        /// <summary>
        /// Resets the value of a bit to 0 (false), given the address of the bit
        /// </summary>
        /// <param name="value">The input value to modify.</param>
        /// <param name="bit">The index (zero based) of the bit to clear.</param>
        public static void ClearBit(ref byte value, int bit)
        {
            value = (byte) (value & ~(1 << bit) & 0xFF);
        }
    }
}
