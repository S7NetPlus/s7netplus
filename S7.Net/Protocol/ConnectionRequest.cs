namespace S7.Net.Protocol
{
    internal static class ConnectionRequest
    {
        public static byte[] GetCOTPConnectionRequest(TsapPair tsapPair)
        {
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
    }
}
