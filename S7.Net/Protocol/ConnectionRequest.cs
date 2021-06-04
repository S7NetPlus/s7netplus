using System;

namespace S7.Net.Protocol
{
    internal static class ConnectionRequest
    {
        public static byte[] GetCOTPConnectionRequest(CpuType cpu, Int16 rack, Int16 slot)
        {
            var tsapPair = GetDefaultTsapPair(cpu, rack, slot);

            byte[] bSend1 = {
                    3, 0, 0, 22, //TPKT
                    17,     //COTP Header Length
                    224,    //Connect Request
                    0, 0,   //Destination Reference
                    0, 46,  //Source Reference
                    0,      //Flags
                    193,    //Parameter Code (src-tasp)
                    2,      //Parameter Length
                    tsapPair.Local.FirstByte, tsapPair.Local.SecondByte,   //Source TASP
                    194,    //Parameter Code (dst-tasp)
                    2,      //Parameter Length
                    tsapPair.Remote.FirstByte, tsapPair.Remote.SecondByte,   //Destination TASP
                    192,    //Parameter Code (tpdu-size)
                    1,      //Parameter Length
                    10      //TPDU Size (2^10 = 1024)
                };

            return bSend1;
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
        /// The <paramref name="rack"/> parameter is greater than 15.
        ///
        /// -or-
        ///
        /// The <paramref name="slot"/> parameter is greater than 15.</exception>
        public static TsapPair GetDefaultTsapPair(CpuType cpuType, int rack, int slot)
        {
            if (rack > 0x0F) throw InvalidRackOrSlot(rack, nameof(rack));
            if (slot > 0x0F) throw InvalidRackOrSlot(slot, nameof(slot));

            switch (cpuType)
            {
                case CpuType.S7200:
                    return new TsapPair(new Tsap(0x10, 0x00), new Tsap(0x10, 0x00));
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

        private static ArgumentOutOfRangeException InvalidRackOrSlot(int value, string name)
        {
            return new ArgumentOutOfRangeException(name,
                $"Invalid {name} value specified (decimal: {value}, hexadecimal: {value:X}), maximum value is 15 (decimal) or 0x0F (hexadecimal).");
        }
    }
}
