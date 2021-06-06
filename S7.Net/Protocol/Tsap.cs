namespace S7.Net.Protocol
{
    /// <summary>
    /// Provides a representation of the Transport Service Access Point, or TSAP in short. TSAP's are used
    /// to specify a client and server address. For most PLC types a default TSAP is available that allows
    /// connection from any IP and can be calculated using the rack and slot numbers.
    /// </summary>
    public struct Tsap
    {
        /// <summary>
        /// First byte of the TSAP.
        /// </summary>
        public byte FirstByte { get; set; }

        /// <summary>
        /// Second byte of the TSAP.
        /// </summary>
        public byte SecondByte { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tsap" /> class using the specified values.
        /// </summary>
        /// <param name="firstByte">The first byte of the TSAP.</param>
        /// <param name="secondByte">The second byte of the TSAP.</param>
        public Tsap(byte firstByte, byte secondByte)
        {
            FirstByte = firstByte;
            SecondByte = secondByte;
        }
    }
}