using System;

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

        /// <summary>
        /// Builds a <see cref="TsapPair" /> that can be used to connect to a PLC using the default connection
        /// addresses.
        /// </summary>
        /// <remarks>
        /// The remote TSAP is constructed using <code>new Tsap(0x03, (byte) ((rack &lt;&lt; 5) | slot))</code>.
        /// </remarks>
        /// <param name="cpuType">The CPU type of the PLC.</param>
        /// <param name="rack">The rack of the PLC's network card.</param>
        /// <param name="slot">The slot of the PLC's network card.</param>
        /// <returns>A TSAP pair that matches the given parameters.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="cpuType"/> is invalid.
        ///
        /// -or-
        ///
        /// The <paramref name="rack"/> parameter is less than 0.
        ///
        /// -or-
        ///
        /// The <paramref name="rack"/> parameter is greater than 15.
        ///
        /// -or-
        ///
        /// The <paramref name="slot"/> parameter is less than 0.
        ///
        /// -or-
        ///
        /// The <paramref name="slot"/> parameter is greater than 15.</exception>
        public static TsapPair GetDefaultTsapPair(CpuType cpuType, int rack, int slot)
        {
            if (rack < 0) throw InvalidRackOrSlot(rack, nameof(rack), "minimum", 0);
            if (rack > 0x0F) throw InvalidRackOrSlot(rack, nameof(rack), "maximum", 0x0F);

            if (slot < 0) throw InvalidRackOrSlot(slot, nameof(slot), "minimum", 0);
            if (slot > 0x0F) throw InvalidRackOrSlot(slot, nameof(slot), "maximum", 0x0F);

            switch (cpuType)
            {
                case CpuType.S7200:
                    return new TsapPair(new Tsap(0x10, 0x00), new Tsap(0x10, 0x01));
                case CpuType.Logo0BA8:
                    // The actual values are probably on a per-project basis
                    return new TsapPair(new Tsap(0x01, 0x00), new Tsap(0x01, 0x02));
                case CpuType.S7200Smart:
                case CpuType.S71200:
                case CpuType.S71500:
                case CpuType.S7300:
                case CpuType.S7400:
                    // Testing with S7 1500 shows only the remote TSAP needs to match. This might differ for other
                    // PLC types.
                    return new TsapPair(new Tsap(0x01, 0x00), new Tsap(0x03, (byte) ((rack << 5) | slot)));
                default:
                    throw new ArgumentOutOfRangeException(nameof(cpuType), "Invalid CPU Type specified");
            }
        }

        private static ArgumentOutOfRangeException InvalidRackOrSlot(int value, string name, string extrema,
            int extremaValue)
        {
            return new ArgumentOutOfRangeException(name,
                $"Invalid {name} value specified (decimal: {value}, hexadecimal: {value:X}), {extrema} value " +
                $"is {extremaValue} (decimal) or {extremaValue:X} (hexadecimal).");
        }
    }
}