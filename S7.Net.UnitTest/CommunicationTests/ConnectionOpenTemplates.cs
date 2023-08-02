namespace S7.Net.UnitTest.CommunicationTests;

internal static class ConnectionOpenTemplates
{
    public static RequestResponsePair ConnectionRequestConfirm { get; } = new RequestResponsePair(
        """
            // TPKT
            03 // Version
            00 // Reserved
            00 16 // Length

            // CR
            11 // Number of bytes following
            E0 // CR / Credit
            00 00 // Destination reference, unused
            __ __ // Source reference, unused
            00 // Class / Option

            // Source TSAP
            C1 // Parameter code
            02 // Parameter length
            TSAP_SRC_CHAN // Channel
            TSAP_SRC_POS // Position

            // Destination TSAP
            C2 // Parameter code
            02 // Parameter length
            TSAP_DEST_CHAN // Channel
            TSAP_DEST_POS // Position

            // PDU Size parameter
            C0 // Parameter code
            01 // Parameter length
            0A // 1024 byte PDU (2 ^ 10)
        """,
        """
            // TPKT
            03 // Version
            00 // Reserved
            00 0B // Length

            // CC
            06 // Length
            D0 // CC / Credit
            00 00 // Destination reference
            00 00 // Source reference
            00 // Class / Option
        """
    );

    public static RequestResponsePair CommunicationSetup { get; } = new RequestResponsePair(
        """
            // TPKT
            03 // Version
            00 // Reserved
            00 19 // Length

            // Data header
            02 // Length
            F0 // Data identifier
            80 // PDU number and end of transmission

            // S7 header
            32 // Protocol ID
            01 // Message type job request
            00 00 // Reserved
            PDU1 PDU2 // PDU reference
            00 08 // Parameter length (Communication Setup)
            00 00 // Data length

            // Communication Setup
            F0 // Function code
            00 // Reserved
            00 03 // Max AMQ caller
            00 03 // Max AMQ callee
            03 C0 // PDU size (960)
        """,
        """
            // TPKT
            03 // Version
            00 // Reserved
            00 1B // Length

            // Data header
            02 // Length
            F0 // Data identifier
            80 // PDU number and end of transmission

            // S7 header
            32 // Protocol ID
            03 // Message type ack data
            00 00 // Reserved
            PDU1 PDU2 // PDU reference
            00 08 // Parameter length (Communication Setup)
            00 00 // Data length
            00 // Error class
            00 // Error code

            // Communication Setup
            F0 // Function code
            00 // Reserved
            00 03 // Max AMQ caller
            00 03 // Max AMQ callee
            03 C0 // PDU size (960)
        """
    );
}