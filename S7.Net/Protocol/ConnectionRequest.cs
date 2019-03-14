using System;

namespace S7.Net.Protocol
{
    internal static class ConnectionRequest
    {
        public static byte[] GetCOTPConnectionRequest(CpuType cpu, Int16 rack, Int16 slot)
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
                    1, 0,   //Source TASP
                    194,    //Parameter Code (dst-tasp)
                    2,      //Parameter Length
                    3, 0,   //Destination TASP
                    192,    //Parameter Code (tpdu-size)
                    1,      //Parameter Length
                    10      //TPDU Size (2^10 = 1024)
                };

            switch (cpu)
            {
                case CpuType.S7200:
                    //S7200: Chr(193) & Chr(2) & Chr(16) & Chr(0) 'Eigener Tsap
                    bSend1[13] = 0x10;
                    bSend1[14] = 0x00;
                    //S7200: Chr(194) & Chr(2) & Chr(16) & Chr(0) 'Fremder Tsap
                    bSend1[17] = 0x10;
                    bSend1[18] = 0x00;
                    break;
                case CpuType.Logo0BA8:
                    // These values are taken from NodeS7, it's not verified if these are
                    // exact requirements to connect to the Logo0BA8.
                    bSend1[13] = 0x01;
                    bSend1[14] = 0x00;
                    bSend1[17] = 0x01;
                    bSend1[18] = 0x02;
                    break;
                case CpuType.S71200:
                case CpuType.S7300:
                case CpuType.S7400:
                    //S7300: Chr(193) & Chr(2) & Chr(1) & Chr(0)  'Eigener Tsap
                    bSend1[13] = 0x01;
                    bSend1[14] = 0x00;
                    //S7300: Chr(194) & Chr(2) & Chr(3) & Chr(2)  'Fremder Tsap
                    bSend1[17] = 0x03;
                    bSend1[18] = (byte) ((rack << 5) | (int) slot);
                    break;
                case CpuType.S71500:
                    // Eigener Tsap
                    bSend1[13] = 0x10;
                    bSend1[14] = 0x02;
                    // Fredmer Tsap
                    bSend1[17] = 0x03;
                    bSend1[18] = (byte) ((rack << 5) | (int) slot);
                    break;
                default:
                    throw new Exception("Wrong CPU Type Secified");
            }
            return bSend1;
        }
    }
}
