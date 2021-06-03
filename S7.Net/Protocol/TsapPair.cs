namespace S7.Net.Protocol
{
    /// <summary>
    /// Implements a pair of TSAP addresses used to connect to a PLC.
    /// </summary>
    public class TsapPair
    {
        /// <summary>
        /// The local <see cref="Tsap" />.
        /// </summary>
        public Tsap Local { get; set; }

        /// <summary>
        /// The remote <see cref="Tsap" />
        /// </summary>
        public Tsap Remote { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TsapPair" /> class using the specified local and
        /// remote TSAP.
        /// </summary>
        /// <param name="local">The local TSAP.</param>
        /// <param name="remote">The remote TSAP.</param>
        public TsapPair(Tsap local, Tsap remote)
        {
            Local = local;
            Remote = remote;
        }
    }
}