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
                    9       //TPDU Size (2^9 = 512)
                };

            switch (cpu)
            {
                case CpuType.S7200:
                    //S7200: Chr(193) & Chr(2) & Chr(16) & Chr(0) 'Eigener Tsap
                    bSend1[11] = 193;
                    bSend1[12] = 2;
                    bSend1[13] = 16;
                    bSend1[14] = 0;
                    //S7200: Chr(194) & Chr(2) & Chr(16) & Chr(0) 'Fremder Tsap
                    bSend1[15] = 194;
                    bSend1[16] = 2;
                    bSend1[17] = 16;
                    bSend1[18] = 0;
                    break;
                case CpuType.S71200:
                case CpuType.S7300:
                    //S7300: Chr(193) & Chr(2) & Chr(1) & Chr(0)  'Eigener Tsap
                    bSend1[11] = 193;
                    bSend1[12] = 2;
                    bSend1[13] = 1;
                    bSend1[14] = 0;
                    //S7300: Chr(194) & Chr(2) & Chr(3) & Chr(2)  'Fremder Tsap
                    bSend1[15] = 194;
                    bSend1[16] = 2;
                    bSend1[17] = 3;
                    bSend1[18] = (byte)(rack * 2 * 16 + slot);
                    break;
                case CpuType.S7400:
                    //S7400: Chr(193) & Chr(2) & Chr(1) & Chr(0)  'Eigener Tsap
                    bSend1[11] = 193;
                    bSend1[12] = 2;
                    bSend1[13] = 1;
                    bSend1[14] = 0;
                    //S7400: Chr(194) & Chr(2) & Chr(3) & Chr(3)  'Fremder Tsap
                    bSend1[15] = 194;
                    bSend1[16] = 2;
                    bSend1[17] = 3;
                    bSend1[18] = (byte)(rack * 2 * 16 + slot);
                    break;
                case CpuType.S71500:
                    // Eigener Tsap
                    bSend1[11] = 193;
                    bSend1[12] = 2;
                    bSend1[13] = 0x10;
                    bSend1[14] = 0x2;
                    // Fredmer Tsap
                    bSend1[15] = 194;
                    bSend1[16] = 2;
                    bSend1[17] = 0x3;
                    bSend1[18] = (byte)(rack * 2 * 16 + slot);
                    break;
                default:
                    throw new Exception("Wrong CPU Type Secified");
            }
            return bSend1;
        }
    }
}
