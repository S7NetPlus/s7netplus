/*=============================================================================|
|  PROJECT SNAP7                                                         1.2.0 |
|==============================================================================|
|  Copyright (C) 2013, 2014 Davide Nardella                                    |
|  All rights reserved.                                                        |
|==============================================================================|
|  SNAP7 is free software: you can redistribute it and/or modify               |
|  it under the terms of the Lesser GNU General Public License as published by |
|  the Free Software Foundation, either version 3 of the License, or           |
|  (at your option) any later version.                                         |
|                                                                              |
|  It means that you can distribute your commercial software linked with       |
|  SNAP7 without the requirement to distribute the source code of your         |
|  application and without the requirement that your application be itself     |
|  distributed under LGPL.                                                     |
|                                                                              |
|  SNAP7 is distributed in the hope that it will be useful,                    |
|  but WITHOUT ANY WARRANTY; without even the implied warranty of              |
|  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the               |
|  Lesser GNU General Public License for more details.                         |
|                                                                              |
|  You should have received a copy of the GNU General Public License and a     |
|  copy of Lesser GNU General Public License along with Snap7.                 |
|  If not, see  http://www.gnu.org/licenses/                                   |
|==============================================================================|
|                                                                              |
|  C# Interface classes.                                                       |
|                                                                              |
|=============================================================================*/
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Snap7
{

    public class S7Consts
    {
        public const string Snap7LibName = "snap7";

        //------------------------------------------------------------------------------
        //                                  PARAMS LIST            
        //------------------------------------------------------------------------------
        public static readonly Int32 p_u16_LocalPort     = 1;
        public static readonly Int32 p_u16_RemotePort    = 2;
        public static readonly Int32 p_i32_PingTimeout   = 3;
        public static readonly Int32 p_i32_SendTimeout   = 4;
        public static readonly Int32 p_i32_RecvTimeout   = 5;
        public static readonly Int32 p_i32_WorkInterval  = 6;
        public static readonly Int32 p_u16_SrcRef        = 7;
        public static readonly Int32 p_u16_DstRef        = 8;
        public static readonly Int32 p_u16_SrcTSap       = 9;
        public static readonly Int32 p_i32_PDURequest    = 10;
        public static readonly Int32 p_i32_MaxClients    = 11;
        public static readonly Int32 p_i32_BSendTimeout  = 12;
        public static readonly Int32 p_i32_BRecvTimeout  = 13;
        public static readonly Int32 p_u32_RecoveryTime  = 14;
        public static readonly Int32 p_u32_KeepAliveTime = 15;
    }
    
    public class S7Client
    {
        #region [Constants, private vars and TypeDefs]
        private const int MsgTextLen = 1024;
        // Error codes
        public static readonly uint errNegotiatingPDU            = 0x00100000;
        public static readonly uint errCliInvalidParams          = 0x00200000;
        public static readonly uint errCliJobPending             = 0x00300000;
        public static readonly uint errCliTooManyItems           = 0x00400000;
        public static readonly uint errCliInvalidWordLen         = 0x00500000;
        public static readonly uint errCliPartialDataWritten     = 0x00600000;
        public static readonly uint errCliSizeOverPDU            = 0x00700000;
        public static readonly uint errCliInvalidPlcAnswer       = 0x00800000;
        public static readonly uint errCliAddressOutOfRange      = 0x00900000;
        public static readonly uint errCliInvalidTransportSize   = 0x00A00000;
        public static readonly uint errCliWriteDataSizeMismatch  = 0x00B00000;
        public static readonly uint errCliItemNotAvailable       = 0x00C00000;
        public static readonly uint errCliInvalidValue           = 0x00D00000;
        public static readonly uint errCliCannotStartPLC         = 0x00E00000;
        public static readonly uint errCliAlreadyRun             = 0x00F00000;
        public static readonly uint errCliCannotStopPLC          = 0x01000000;
        public static readonly uint errCliCannotCopyRamToRom     = 0x01100000;
        public static readonly uint errCliCannotCompress         = 0x01200000;
        public static readonly uint errCliAlreadyStop            = 0x01300000;
        public static readonly uint errCliFunNotAvailable        = 0x01400000;
        public static readonly uint errCliUploadSequenceFailed   = 0x01500000;
        public static readonly uint errCliInvalidDataSizeRecvd   = 0x01600000;
        public static readonly uint errCliInvalidBlockType       = 0x01700000;
        public static readonly uint errCliInvalidBlockNumber     = 0x01800000;
        public static readonly uint errCliInvalidBlockSize       = 0x01900000;
        public static readonly uint errCliDownloadSequenceFailed = 0x01A00000;
        public static readonly uint errCliInsertRefused          = 0x01B00000;
        public static readonly uint errCliDeleteRefused          = 0x01C00000;
        public static readonly uint errCliNeedPassword           = 0x01D00000;
        public static readonly uint errCliInvalidPassword        = 0x01E00000;
        public static readonly uint errCliNoPasswordToSetOrClear = 0x01F00000;
        public static readonly uint errCliJobTimeout             = 0x02000000;
        public static readonly uint errCliPartialDataRead        = 0x02100000;
        public static readonly uint errCliBufferTooSmall         = 0x02200000;
        public static readonly uint errCliFunctionRefused        = 0x02300000;
        public static readonly uint errCliDestroying             = 0x02400000;
        public static readonly uint errCliInvalidParamNumber     = 0x02500000;
        public static readonly uint errCliCannotChangeParam      = 0x02600000;
        
        // Area ID
        public static readonly byte S7AreaPE = 0x81;
        public static readonly byte S7AreaPA = 0x82;
        public static readonly byte S7AreaMK = 0x83;
        public static readonly byte S7AreaDB = 0x84;
        public static readonly byte S7AreaCT = 0x1C;
        public static readonly byte S7AreaTM = 0x1D;

        // Word Length
        public static readonly int S7WLBit     = 0x01;
        public static readonly int S7WLByte    = 0x02;
        public static readonly int S7WLWord    = 0x04;
        public static readonly int S7WLDWord   = 0x06;
        public static readonly int S7WLReal    = 0x08;
        public static readonly int S7WLCounter = 0x1C;
        public static readonly int S7WLTimer   = 0x1D;

        // Block type
        public static readonly byte Block_OB  = 0x38;
        public static readonly byte Block_DB  = 0x41;
        public static readonly byte Block_SDB = 0x42;
        public static readonly byte Block_FC  = 0x43;
        public static readonly byte Block_SFC = 0x44;
        public static readonly byte Block_FB  = 0x45;
        public static readonly byte Block_SFB = 0x46;

        // Sub Block Type 
        public static readonly byte SubBlk_OB  = 0x08;
        public static readonly byte SubBlk_DB  = 0x0A;
        public static readonly byte SubBlk_SDB = 0x0B;
        public static readonly byte SubBlk_FC  = 0x0C;
        public static readonly byte SubBlk_SFC = 0x0D;
        public static readonly byte SubBlk_FB  = 0x0E;
        public static readonly byte SubBlk_SFB = 0x0F;

        // Block languages
        public static readonly byte BlockLangAWL   = 0x01;
        public static readonly byte BlockLangKOP   = 0x02;
        public static readonly byte BlockLangFUP   = 0x03;
        public static readonly byte BlockLangSCL   = 0x04;
        public static readonly byte BlockLangDB    = 0x05;
        public static readonly byte BlockLangGRAPH = 0x06;

        // Max number of vars (multiread/write)
        public static readonly int MaxVars = 20;

        // Client Connection Type
        public static readonly UInt16 CONNTYPE_PG    = 0x01;  // Connect to the PLC as a PG
        public static readonly UInt16 CONNTYPE_OP    = 0x02;  // Connect to the PLC as an OP
        public static readonly UInt16 CONNTYPE_BASIC = 0x03;  // Basic connection 

        // Job
        private const int JobComplete = 0;
        private const int JobPending = 1;

        private IntPtr Client;

        // Data Item 
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct S7DataItem
        {
            public Int32 Area;
            public Int32 WordLen;
            public Int32 Result;
            public Int32 DBNumber;
            public Int32 Start;
            public Int32 Amount;
            public IntPtr pData;
        }

        // Block List
        [StructLayout(LayoutKind.Sequential, Pack = 1)] // <- "maybe" we don't need
        public struct S7BlocksList
        {
            public Int32 OBCount;
            public Int32 FBCount;
            public Int32 FCCount;
            public Int32 SFBCount;
            public Int32 SFCCount;
            public Int32 DBCount;
            public Int32 SDBCount;
        };

        // Packed Block Info
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        protected struct US7BlockInfo
        {
            public Int32 BlkType;
            public Int32 BlkNumber;
            public Int32 BlkLang;
            public Int32 BlkFlags;
            public Int32 MC7Size;  // The real size in bytes
            public Int32 LoadSize;
            public Int32 LocalData;
            public Int32 SBBLength;
            public Int32 CheckSum;
            public Int32 Version;
            // Chars info
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public char[] CodeDate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public char[] IntfDate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public char[] Author;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public char[] Family;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public char[] Header;
        };
        private US7BlockInfo UBlockInfo;

        // Managed Block Info
        public struct S7BlockInfo
        {
            public int BlkType;
            public int BlkNumber;
            public int BlkLang;
            public int BlkFlags;
            public int MC7Size;  // The real size in bytes
            public int LoadSize;
            public int LocalData;
            public int SBBLength;
            public int CheckSum;
            public int Version;
            // Chars info
            public string CodeDate;
            public string IntfDate;
            public string Author;
            public string Family;
            public string Header;
        };


        public ushort[] TS7BlocksOfType;

        // Packed Order Code + Version
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        protected struct US7OrderCode
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
            public char[] Code;
            public byte V1;
            public byte V2;
            public byte V3;
        };
        private US7OrderCode UOrderCode;

        // Managed Order Code + Version
        public struct S7OrderCode
        {
            public string Code;
            public byte V1;
            public byte V2;
            public byte V3;
        };

        // Packed CPU Info
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        protected struct US7CpuInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 33)]
            public char[] ModuleTypeName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
            public char[] SerialNumber;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
            public char[] ASName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 27)]
            public char[] Copyright;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
            public char[] ModuleName;
        };
        private US7CpuInfo UCpuInfo;

        // Managed CPU Info
        public struct S7CpuInfo
        {
            public string ModuleTypeName;
            public string SerialNumber;
            public string ASName;
            public string Copyright;
            public string ModuleName;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct S7CpInfo
        {
            public Int32 MaxPduLengt;
            public Int32 MaxConnections;
            public Int32 MaxMpiRate;
            public Int32 MaxBusRate;
        };

        // See §33.1 of "System Software for S7-300/400 System and Standard Functions"
        // and see SFC51 description too
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SZL_HEADER
        {
            public UInt16 LENTHDR;
            public UInt16 N_DR;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct S7SZL
        {
            public SZL_HEADER Header;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x4000 - 4)]
            public byte[] Data;
        };

        // SZL List of available SZL IDs : same as SZL but List items are big-endian adjusted
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct S7SZLList
        {
            public SZL_HEADER Header;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x2000 - 2)]
            public UInt16[] Data;
        };

        // S7 Protection
        // See §33.19 of "System Software for S7-300/400 System and Standard Functions"
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct S7Protection  // Packed S7Protection
        {
            public UInt16 sch_schal;
            public UInt16 sch_par;
            public UInt16 sch_rel;
            public UInt16 bart_sch;
            public UInt16 anl_sch;
        };

        // C++ time struct, functions to convert it from/to DateTime are provided ;-)
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        protected struct cpp_tm
        {
            public Int32 tm_sec;
            public Int32 tm_min;
            public Int32 tm_hour;
            public Int32 tm_mday;
            public Int32 tm_mon;
            public Int32 tm_year;
            public Int32 tm_wday;
            public Int32 tm_yday;
            public Int32 tm_isdst;
        }
        private cpp_tm tm;

        #endregion

        #region [Class Control]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern IntPtr Cli_Create();
        public S7Client()
        {
            Client = Cli_Create();
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_Destroy(ref IntPtr Client);
        ~S7Client()
        {
            Cli_Destroy(ref Client);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_Connect(IntPtr Client);
        public int Connect()
        {
            return Cli_Connect(Client);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_ConnectTo(IntPtr Client,
            [MarshalAs(UnmanagedType.LPStr)] string Address,
            int Rack,
            int Slot
            );
        
        public int ConnectTo(string Address, int Rack, int Slot)
        {
            return Cli_ConnectTo(Client, Address, Rack, Slot);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_SetConnectionParams(IntPtr Client,
            [MarshalAs(UnmanagedType.LPStr)] string Address,
            UInt16 LocalTSAP,
            UInt16 RemoteTSAP
            );

        public int SetConnectionParams(string Address, UInt16 LocalTSAP, UInt16 RemoteTSAP)
        {
            return Cli_SetConnectionParams(Client, Address, LocalTSAP, RemoteTSAP);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_SetConnectionType(IntPtr Client, UInt16 ConnectionType);
        public int SetConnectionType(UInt16 ConnectionType)
        {
            return Cli_SetConnectionType(Client, ConnectionType);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_Disconnect(IntPtr Client);
        public int Disconnect()
        {
            return Cli_Disconnect(Client);
        }

        // Get/SetParam needs a void* parameter, internally it decides the kind of pointer
        // in accord to ParamNumber.
        // To avoid the use of unsafe code we split the DLL functions and use overloaded methods.

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Cli_GetParam")]
        protected static extern int Cli_GetParam_i16(IntPtr Client, Int32 ParamNumber, ref Int16 IntValue);
        public int GetParam(Int32 ParamNumber, ref Int16 IntValue)
        {
            return Cli_GetParam_i16(Client, ParamNumber, ref IntValue);
        }
        
        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Cli_GetParam")]
        protected static extern int Cli_GetParam_u16(IntPtr Client, Int32 ParamNumber, ref UInt16 IntValue);
        public int GetParam(Int32 ParamNumber, ref UInt16 IntValue)
        {
            return Cli_GetParam_u16(Client, ParamNumber, ref IntValue);
        }
        
        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Cli_GetParam")]
        protected static extern int Cli_GetParam_i32(IntPtr Client, Int32 ParamNumber, ref Int32 IntValue);
        public int GetParam(Int32 ParamNumber, ref Int32 IntValue)
        {
            return Cli_GetParam_i32(Client, ParamNumber, ref IntValue);
        }
        
        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Cli_GetParam")]
        protected static extern int Cli_GetParam_u32(IntPtr Client, Int32 ParamNumber, ref UInt32 IntValue);
        public int GetParam(Int32 ParamNumber, ref UInt32 IntValue)
        {
            return Cli_GetParam_u32(Client, ParamNumber, ref IntValue);
        }
        
        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Cli_GetParam")]
        protected static extern int Cli_GetParam_i64(IntPtr Client, Int32 ParamNumber, ref Int64 IntValue);
        public int GetParam(Int32 ParamNumber, ref Int64 IntValue)
        {
            return Cli_GetParam_i64(Client, ParamNumber, ref IntValue);
        }
        
        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Cli_GetParam")]
        protected static extern int Cli_GetParam_u64(IntPtr Client, Int32 ParamNumber, ref UInt64 IntValue);
        public int GetParam(Int32 ParamNumber, ref UInt64 IntValue)
        {
            return Cli_GetParam_u64(Client, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Cli_SetParam")]
        protected static extern int Cli_SetParam_i16(IntPtr Client, Int32 ParamNumber, ref Int16 IntValue);
        public int SetParam(Int32 ParamNumber, ref Int16 IntValue)
        {
            return Cli_SetParam_i16(Client, ParamNumber, ref IntValue);
        }
        
        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Cli_SetParam")]
        protected static extern int Cli_SetParam_u16(IntPtr Client, Int32 ParamNumber, ref UInt16 IntValue);
        public int SetParam(Int32 ParamNumber, ref UInt16 IntValue)
        {
            return Cli_SetParam_u16(Client, ParamNumber, ref IntValue);
        }
        
        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Cli_SetParam")]
        protected static extern int Cli_SetParam_i32(IntPtr Client, Int32 ParamNumber, ref Int32 IntValue);
        public int SetParam(Int32 ParamNumber, ref Int32 IntValue)
        {
            return Cli_SetParam_i32(Client, ParamNumber, ref IntValue);
        }
        
        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Cli_SetParam")]
        protected static extern int Cli_SetParam_u32(IntPtr Client, Int32 ParamNumber, ref UInt32 IntValue);
        public int SetParam(Int32 ParamNumber, ref UInt32 IntValue)
        {
            return Cli_SetParam_u32(Client, ParamNumber, ref IntValue);
        }
        
        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Cli_SetParam")]
        protected static extern int Cli_SetParam_i64(IntPtr Client, Int32 ParamNumber, ref Int64 IntValue);
        public int SetParam(Int32 ParamNumber, ref Int64 IntValue)
        {
            return Cli_SetParam_i64(Client, ParamNumber, ref IntValue);
        }
        
        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Cli_SetParam")]
        protected static extern int Cli_SetParam_u64(IntPtr Client, Int32 ParamNumber, ref UInt64 IntValue);
        public int SetParam(Int32 ParamNumber, ref UInt64 IntValue)
        {
            return Cli_SetParam_u64(Client, ParamNumber, ref IntValue);
        }

        public delegate void S7CliCompletion(IntPtr usrPtr, int opCode, int opResult);

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_SetAsCallback(IntPtr Client, S7CliCompletion Completion, IntPtr usrPtr);
        public int SetAsCallBack(S7CliCompletion Completion, IntPtr usrPtr)
        {
            return Cli_SetAsCallback(Client, Completion, usrPtr);
        }
        
        #endregion

        #region [Data I/O main functions]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_ReadArea(IntPtr Client, int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer);
        public int ReadArea(int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer)
        {
            return Cli_ReadArea(Client, Area, DBNumber, Start, Amount, WordLen, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_WriteArea(IntPtr Client, int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer);
        public int WriteArea(int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer)
        {
            return Cli_WriteArea(Client, Area, DBNumber, Start, Amount, WordLen, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_ReadMultiVars(IntPtr Client, ref S7DataItem Item, int ItemsCount);
        public int ReadMultiVars(ref S7DataItem Item, int ItemsCount)
        {
            return Cli_ReadMultiVars(Client, ref Item, ItemsCount);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_WriteMultiVars(IntPtr Client, ref S7DataItem Item, int ItemsCount);
        public int WriteMultiVars(ref S7DataItem Item, int ItemsCount)
        {
            return Cli_WriteMultiVars(Client, ref Item, ItemsCount);
        }

        #endregion

        #region [Data I/O lean functions]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_DBRead(IntPtr Client, int DBNumber, int Start, int Size, byte[] Buffer);
        public int DBRead(int DBNumber, int Start, int Size, byte[] Buffer)
        {
            return Cli_DBRead(Client, DBNumber, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_DBWrite(IntPtr Client, int DBNumber, int Start, int Size, byte[] Buffer);
        public int DBWrite(int DBNumber, int Start, int Size, byte[] Buffer)
        {
            return Cli_DBWrite(Client, DBNumber, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_MBRead(IntPtr Client, int Start, int Size, byte[] Buffer);
        public int MBRead(int Start, int Size, byte[] Buffer)
        {
            return Cli_MBRead(Client, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_MBWrite(IntPtr Client, int Start, int Size, byte[] Buffer);
        public int MBWrite(int Start, int Size, byte[] Buffer)
        {
            return Cli_MBWrite(Client, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_EBRead(IntPtr Client, int Start, int Size, byte[] Buffer);
        public int EBRead(int Start, int Size, byte[] Buffer)
        {
            return Cli_EBRead(Client, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_EBWrite(IntPtr Client, int Start, int Size, byte[] Buffer);
        public int EBWrite(int Start, int Size, byte[] Buffer)
        {
            return Cli_EBWrite(Client, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_ABRead(IntPtr Client, int Start, int Size, byte[] Buffer);
        public int ABRead(int Start, int Size, byte[] Buffer)
        {
            return Cli_ABRead(Client, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_ABWrite(IntPtr Client, int Start, int Size, byte[] Buffer);
        public int ABWrite(int Start, int Size, byte[] Buffer)
        {
            return Cli_ABWrite(Client, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_TMRead(IntPtr Client, int Start, int Amount, ushort[] Buffer);
        public int TMRead(int Start, int Amount, ushort[] Buffer)
        {
            return Cli_TMRead(Client, Start, Amount, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_TMWrite(IntPtr Client, int Start, int Amount, ushort[] Buffer);
        public int TMWrite(int Start, int Amount, ushort[] Buffer)
        {
            return Cli_TMWrite(Client, Start, Amount, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_CTRead(IntPtr Client, int Start, int Amount, ushort[] Buffer);
        public int CTRead(int Start, int Amount, ushort[] Buffer)
        {
            return Cli_CTRead(Client, Start, Amount, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_CTWrite(IntPtr Client, int Start, int Amount, ushort[] Buffer);
        public int CTWrite(int Start, int Amount, ushort[] Buffer)
        {
            return Cli_CTWrite(Client, Start, Amount, Buffer);
        }

        #endregion

        #region [Directory functions]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_ListBlocks(IntPtr Client, ref S7BlocksList List);
        public int ListBlocks(ref S7BlocksList List)
        {
            return Cli_ListBlocks(Client, ref List);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_GetAgBlockInfo(IntPtr Client, int BlockType, int BlockNum, ref US7BlockInfo Info);
        public int GetAgBlockInfo(IntPtr Client, int BlockType, int BlockNum, ref S7BlockInfo Info)
        {
            int res = Cli_GetAgBlockInfo(Client, BlockType, BlockNum, ref UBlockInfo);
            // Packed->Managed
            if (res == 0)
            {
                Info.BlkType = UBlockInfo.BlkType;
                Info.BlkNumber = UBlockInfo.BlkNumber;
                Info.BlkLang = UBlockInfo.BlkLang;
                Info.BlkFlags = UBlockInfo.BlkFlags;
                Info.MC7Size = UBlockInfo.MC7Size;
                Info.LoadSize = UBlockInfo.LoadSize;
                Info.LocalData = UBlockInfo.LocalData;
                Info.SBBLength = UBlockInfo.SBBLength;
                Info.CheckSum = UBlockInfo.CheckSum;
                Info.Version = UBlockInfo.Version;
                // Chars info
                Info.CodeDate = new string(UBlockInfo.CodeDate);
                Info.IntfDate = new string(UBlockInfo.IntfDate);
                Info.Author = new string(UBlockInfo.Author);
                Info.Family = new string(UBlockInfo.Family);
                Info.Header = new string(UBlockInfo.Header);
            }
            return res;
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_GetPgBlockInfo(IntPtr Client, ref US7BlockInfo Info, byte[] Buffer, Int32 Size);
        public int GetPgBlockInfo(ref S7BlockInfo Info, byte[] Buffer, int Size)
        {
            int res = Cli_GetPgBlockInfo(Client, ref UBlockInfo, Buffer, Size);
            // Packed->Managed
            if (res == 0)
            {
                Info.BlkType = UBlockInfo.BlkType;
                Info.BlkNumber = UBlockInfo.BlkNumber;
                Info.BlkLang = UBlockInfo.BlkLang;
                Info.BlkFlags = UBlockInfo.BlkFlags;
                Info.MC7Size = UBlockInfo.MC7Size;
                Info.LoadSize = UBlockInfo.LoadSize;
                Info.LocalData = UBlockInfo.LocalData;
                Info.SBBLength = UBlockInfo.SBBLength;
                Info.CheckSum = UBlockInfo.CheckSum;
                Info.Version = UBlockInfo.Version;
                // Chars info
                Info.CodeDate = new string(UBlockInfo.CodeDate);
                Info.IntfDate = new string(UBlockInfo.IntfDate);
                Info.Author = new string(UBlockInfo.Author);
                Info.Family = new string(UBlockInfo.Family);
                Info.Header = new string(UBlockInfo.Header);
            }
            return res;
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_ListBlocksOfType(IntPtr Client, Int32 BlockType, ushort[] List, ref int ItemsCount);
        public int ListBlocksOfType(int BlockType, ushort[] List, ref int ItemsCount)
        {
            return Cli_ListBlocksOfType(Client, BlockType, List, ref ItemsCount);
        }

        #endregion

        #region [Blocks functions]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_Upload(IntPtr Client, int BlockType, int BlockNum, byte[] UsrData, ref int Size);
        public int Upload(int BlockType, int BlockNum, byte[] UsrData, ref int Size)
        {
            return Cli_Upload(Client, BlockType, BlockNum, UsrData, ref Size);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_FullUpload(IntPtr Client, int BlockType, int BlockNum, byte[] UsrData, ref int Size);
        public int FullUpload(int BlockType, int BlockNum, byte[] UsrData, ref int Size)
        {
            return Cli_FullUpload(Client, BlockType, BlockNum, UsrData, ref Size);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_Download(IntPtr Client, int BlockNum, byte[] UsrData, int Size);
        public int Download(int BlockNum, byte[] UsrData, int Size)
        {
            return Cli_Download(Client, BlockNum, UsrData, Size);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_Delete(IntPtr Client, int BlockType, int BlockNum);
        public int Delete(int BlockType, int BlockNum)
        {
            return Cli_Delete(Client, BlockType, BlockNum);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_DBGet(IntPtr Client, int DBNumber, byte[] UsrData, ref int Size);
        public int DBGet(int DBNumber, byte[] UsrData, ref int Size)
        {
            return Cli_DBGet(Client, DBNumber, UsrData, ref Size);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_DBFill(IntPtr Client, int DBNumber, int FillChar);
        public int DBFill(int DBNumber, int FillChar)
        {
            return Cli_DBFill(Client, DBNumber, FillChar);
        }

        #endregion

        #region [Date/Time functions]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_GetPlcDateTime(IntPtr Client, ref cpp_tm tm);
        public int GetPlcDateTime(ref DateTime DT)
        {           
            int res = Cli_GetPlcDateTime(Client, ref tm);
            if (res == 0)
            {
                // Packed->Managed
                DateTime PlcDT = new DateTime(tm.tm_year+1900, tm.tm_mon+1, tm.tm_mday, tm.tm_hour, tm.tm_min, tm.tm_sec);
                DT = PlcDT;
            }
            return res;
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_SetPlcDateTime(IntPtr Client, ref cpp_tm tm);
        public int SetPlcDateTime(DateTime DT)
        {

            // Managed->Packed
            tm.tm_year = DT.Year - 1900;
            tm.tm_mon = DT.Month-1;
            tm.tm_mday = DT.Day;
            tm.tm_hour = DT.Hour;
            tm.tm_min = DT.Minute;
            tm.tm_sec = DT.Second;

            return Cli_SetPlcDateTime(Client, ref tm);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_SetPlcSystemDateTime(IntPtr Client);
        public int SetPlcSystemDateTime()
        {
            return Cli_SetPlcSystemDateTime(Client);
        }      
        
        #endregion

        #region [System Info functions]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_GetOrderCode(IntPtr Client, ref US7OrderCode Info);
        public int GetOrderCode(ref S7OrderCode Info)
        {
            int res = Cli_GetOrderCode(Client, ref UOrderCode);
            // Packed->Managed
            if (res == 0)
            {
                Info.Code = new string(UOrderCode.Code);
                Info.V1 = UOrderCode.V1;
                Info.V2 = UOrderCode.V2;
                Info.V3 = UOrderCode.V3;
            }
            return res;
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_GetCpuInfo(IntPtr Client, ref US7CpuInfo Info);
        public int GetCpuInfo(ref S7CpuInfo Info)
        {
            int res = Cli_GetCpuInfo(Client, ref UCpuInfo);
            // Packed->Managed
            if (res == 0)
            {
                Info.ModuleTypeName = new string(UCpuInfo.ModuleTypeName);
                Info.SerialNumber = new string(UCpuInfo.SerialNumber);
                Info.ASName = new string(UCpuInfo.ASName);
                Info.Copyright = new string(UCpuInfo.Copyright);
                Info.ModuleName = new string(UCpuInfo.ModuleName);
            }
            return res;
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_GetCpInfo(IntPtr Client, ref S7CpInfo Info);

        public int GetCpInfo(ref S7CpInfo Info)
        {
            return Cli_GetCpInfo(Client, ref Info);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_ReadSZL(IntPtr Client, int ID, int Index, ref S7SZL Data, ref Int32 Size);
        public int ReadSZL(int ID, int Index, ref S7SZL Data, ref Int32 Size)
        {
            return Cli_ReadSZL(Client, ID, Index, ref Data, ref Size);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_ReadSZLList(IntPtr Client, ref S7SZLList List, ref Int32 ItemsCount);
        public int ReadSZLList(ref S7SZLList List, ref Int32 ItemsCount)
        {
            return Cli_ReadSZLList(Client, ref List, ref ItemsCount);
        }

        #endregion

        #region [Control functions]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_PlcHotStart(IntPtr Client);
        public int PlcHotStart()
        {
            return Cli_PlcHotStart(Client);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_PlcColdStart(IntPtr Client);
        public int PlcColdStart()
        {
            return Cli_PlcColdStart(Client);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_PlcStop(IntPtr Client);
        public int PlcStop()
        {
            return Cli_PlcStop(Client);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_CopyRamToRom(IntPtr Client, UInt32 Timeout);
        public int PlcCopyRamToRom(UInt32 Timeout)
        {
            return Cli_CopyRamToRom(Client, Timeout);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_Compress(IntPtr Client, UInt32 Timeout);
        public int PlcCompress(UInt32 Timeout)
        {
            return Cli_Compress(Client, Timeout);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_GetPlcStatus(IntPtr Client, ref Int32 Status);
        public int PlcGetStatus(ref Int32 Status)
        {
            return Cli_GetPlcStatus(Client, ref Status);
        }

        #endregion

        #region [Security functions]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_GetProtection(IntPtr Client, ref S7Protection Protection);
        public int GetProtection(ref S7Protection Protection)
        {
            return Cli_GetProtection(Client, ref Protection);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_SetSessionPassword(IntPtr Client, [MarshalAs(UnmanagedType.LPStr)] string Password);
        public int SetSessionPassword(string Password)
        {
            return Cli_SetSessionPassword(Client, Password);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_ClearSessionPassword(IntPtr Client);
        public int ClearSessionPassword()
        {
            return Cli_ClearSessionPassword(Client);
        }

        #endregion

        #region [Low Level]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_IsoExchangeBuffer(IntPtr Client, byte[] Buffer, ref Int32 Size);
        public int IsoExchangeBuffer(byte[] Buffer, ref Int32 Size)
        {
            return Cli_IsoExchangeBuffer(Client, Buffer, ref Size);
        }

        #endregion

        #region [Async functions]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsReadArea(IntPtr Client, int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer);
        public int AsReadArea(int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer)
        {
            return Cli_AsReadArea(Client, Area, DBNumber, Start, Amount, WordLen, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsWriteArea(IntPtr Client, int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer);
        public int AsWriteArea(int Area, int DBNumber, int Start, int Amount, int WordLen, byte[] Buffer)
        {
            return Cli_AsWriteArea(Client, Area, DBNumber, Start, Amount, WordLen, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsDBRead(IntPtr Client, int DBNumber, int Start, int Size, byte[] Buffer);
        public int AsDBRead(int DBNumber, int Start, int Size, byte[] Buffer)
        {
            return Cli_AsDBRead(Client, DBNumber, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsDBWrite(IntPtr Client, int DBNumber, int Start, int Size, byte[] Buffer);
        public int AsDBWrite(int DBNumber, int Start, int Size, byte[] Buffer)
        {
            return Cli_AsDBWrite(Client, DBNumber, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsMBRead(IntPtr Client, int Start, int Size, byte[] Buffer);
        public int AsMBRead(int Start, int Size, byte[] Buffer)
        {
            return Cli_AsMBRead(Client, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsMBWrite(IntPtr Client, int Start, int Size, byte[] Buffer);
        public int AsMBWrite(int Start, int Size, byte[] Buffer)
        {
            return Cli_AsMBWrite(Client, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsEBRead(IntPtr Client, int Start, int Size, byte[] Buffer);
        public int AsEBRead(int Start, int Size, byte[] Buffer)
        {
            return Cli_AsEBRead(Client, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsEBWrite(IntPtr Client, int Start, int Size, byte[] Buffer);
        public int AsEBWrite(int Start, int Size, byte[] Buffer)
        {
            return Cli_AsEBWrite(Client, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsABRead(IntPtr Client, int Start, int Size, byte[] Buffer);
        public int AsABRead(int Start, int Size, byte[] Buffer)
        {
            return Cli_AsABRead(Client, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsABWrite(IntPtr Client, int Start, int Size, byte[] Buffer);
        public int AsABWrite(int Start, int Size, byte[] Buffer)
        {
            return Cli_AsABWrite(Client, Start, Size, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsTMRead(IntPtr Client, int Start, int Amount, ushort[] Buffer);
        public int AsTMRead(int Start, int Amount, ushort[] Buffer)
        {
            return Cli_AsTMRead(Client, Start, Amount, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsTMWrite(IntPtr Client, int Start, int Amount, ushort[] Buffer);
        public int AsTMWrite(int Start, int Amount, ushort[] Buffer)
        {
            return Cli_AsTMWrite(Client, Start, Amount, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsCTRead(IntPtr Client, int Start, int Amount, ushort[] Buffer);
        public int AsCTRead(int Start, int Amount, ushort[] Buffer)
        {
            return Cli_AsCTRead(Client, Start, Amount, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsCTWrite(IntPtr Client, int Start, int Amount, ushort[] Buffer);
        public int AsCTWrite(int Start, int Amount, ushort[] Buffer)
        {
            return Cli_AsCTWrite(Client, Start, Amount, Buffer);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsListBlocksOfType(IntPtr Client, Int32 BlockType, ushort[] List);
        public int AsListBlocksOfType(int BlockType, ushort[] List)
        {
            return Cli_AsListBlocksOfType(Client, BlockType, List);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsReadSZL(IntPtr Client, int ID, int Index, ref S7SZL Data, ref Int32 Size);
        public int AsReadSZL(int ID, int Index, ref S7SZL Data, ref Int32 Size)
        {
            return Cli_AsReadSZL(Client, ID, Index, ref Data, ref Size);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsReadSZLList(IntPtr Client, ref S7SZLList List, ref Int32 ItemsCount);
        public int AsReadSZLList(ref S7SZLList List, ref Int32 ItemsCount)
        {
            return Cli_AsReadSZLList(Client, ref List, ref ItemsCount);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsUpload(IntPtr Client, int BlockType, int BlockNum, byte[] UsrData, ref int Size);
        public int AsUpload(int BlockType, int BlockNum, byte[] UsrData, ref int Size)
        {
            return Cli_AsUpload(Client, BlockType, BlockNum, UsrData, ref Size);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsFullUpload(IntPtr Client, int BlockType, int BlockNum, byte[] UsrData, ref int Size);
        public int AsFullUpload(int BlockType, int BlockNum, byte[] UsrData, ref int Size)
        {
            return Cli_AsFullUpload(Client, BlockType, BlockNum, UsrData, ref Size);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsDownload(IntPtr Client, int BlockNum, byte[] UsrData, int Size);
        public int ASDownload(int BlockNum, byte[] UsrData, int Size)
        {
            return Cli_AsDownload(Client, BlockNum, UsrData, Size);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsPlcCopyRamToRom(IntPtr Client, UInt32 Timeout);
        public int AsPlcCopyRamToRom(UInt32 Timeout)
        {
            return Cli_AsPlcCopyRamToRom(Client, Timeout);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsPlcCompress(IntPtr Client, UInt32 Timeout);
        public int AsPlcCompress(UInt32 Timeout)
        {
            return Cli_AsPlcCompress(Client, Timeout);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsDBGet(IntPtr Client, int DBNumber, byte[] UsrData, ref int Size);
        public int AsDBGet(int DBNumber, byte[] UsrData, ref int Size)
        {
            return Cli_AsDBGet(Client, DBNumber, UsrData, ref Size);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_AsDBFill(IntPtr Client, int DBNumber, int FillChar);
        public int AsDBFill(int DBNumber, int FillChar)
        {
            return Cli_AsDBFill(Client, DBNumber, FillChar);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_CheckAsCompletion(IntPtr Client, ref Int32 opResult);
        public bool CheckAsCompletion(ref int opResult)
        {
            return Cli_CheckAsCompletion(Client, ref opResult) == JobComplete;
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_WaitAsCompletion(IntPtr Client, Int32 Timeout);
        public int WaitAsCompletion(int Timeout)
        {
            return Cli_WaitAsCompletion(Client, Timeout);
        }

        #endregion

        #region [Info Functions]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_GetExecTime(IntPtr Client, ref UInt32 Time);
        public int ExecTime()
        {
            UInt32 Time = new UInt32();
            if (Cli_GetExecTime(Client, ref Time) == 0)
                return (int)(Time);
            else
                return -1;
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_GetLastError(IntPtr Client, ref Int32 LastError);
        public int LastError()
        {
            Int32 ClientLastError = new Int32();
            if (Cli_GetLastError(Client, ref ClientLastError) == 0)
                return (int)ClientLastError;
            else
                return -1;
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_GetPduLength(IntPtr Client, ref Int32 Requested, ref Int32 Negotiated);

        public int RequestedPduLength()
        {
            Int32 Requested = new Int32();
            Int32 Negotiated = new Int32();
            if (Cli_GetPduLength(Client, ref Requested, ref Negotiated) == 0)
                return Requested;
            else
                return -1;
        }

        public int NegotiatedPduLength()
        {
            Int32 Requested = new Int32();
            Int32 Negotiated = new Int32();
            if (Cli_GetPduLength(Client, ref Requested, ref Negotiated) == 0)
                return Negotiated;
            else
                return -1;
        }

        [DllImport(S7Consts.Snap7LibName, CharSet = CharSet.Ansi)]
        protected static extern int Cli_ErrorText(int Error, StringBuilder ErrMsg, int TextSize);
        public string ErrorText(int Error)
        {
            StringBuilder Message = new StringBuilder(MsgTextLen);
            Cli_ErrorText(Error, Message, MsgTextLen);
            return Message.ToString();
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Cli_GetConnected(IntPtr Client, ref UInt32 IsConnected);
        public bool Connected()
        {
            UInt32 IsConnected = new UInt32();
            if (Cli_GetConnected(Client, ref IsConnected) == 0)
                return IsConnected!=0;
            else
                return false;
        }

        #endregion
    }

    public class S7Server
    {
        #region [Constants, private vars and TypeDefs]

        private const int MsgTextLen = 1024;
        private const int mkEvent = 0;
        private const int mkLog   = 1;

        // Server Area ID  (use with Register/unregister - Lock/unlock Area)
        public static readonly int srvAreaPE = 0;
        public static readonly int srvAreaPA = 1;
        public static readonly int srvAreaMK = 2;
        public static readonly int srvAreaCT = 3;
        public static readonly int srvAreaTM = 4;
        public static readonly int srvAreaDB = 5;
        // Errors
        public static readonly uint errSrvCannotStart        = 0x00100000; // Server cannot start
        public static readonly uint errSrvDBNullPointer      = 0x00200000; // Passed null as PData
        public static readonly uint errSrvAreaAlreadyExists  = 0x00300000; // Area Re-registration
        public static readonly uint errSrvUnknownArea        = 0x00400000; // Unknown area
        public static readonly uint errSrvInvalidParams      = 0x00500000; // Invalid param(s) supplied
        public static readonly uint errSrvTooManyDB          = 0x00600000; // Cannot register DB
        public static readonly uint errSrvInvalidParamNumber = 0x00700000; // Invalid param (srv_get/set_param)
        public static readonly uint errSrvCannotChangeParam  = 0x00800000; // Cannot change because running

        // TCP Server Event codes
        public static readonly uint evcServerStarted       = 0x00000001;
        public static readonly uint evcServerStopped       = 0x00000002;
        public static readonly uint evcListenerCannotStart = 0x00000004;
        public static readonly uint evcClientAdded         = 0x00000008;
        public static readonly uint evcClientRejected      = 0x00000010;
        public static readonly uint evcClientNoRoom        = 0x00000020;
        public static readonly uint evcClientException     = 0x00000040;
        public static readonly uint evcClientDisconnected  = 0x00000080;
        public static readonly uint evcClientTerminated    = 0x00000100;
        public static readonly uint evcClientsDropped      = 0x00000200;
        public static readonly uint evcReserved_00000400   = 0x00000400; // actually unused
        public static readonly uint evcReserved_00000800   = 0x00000800; // actually unused
        public static readonly uint evcReserved_00001000   = 0x00001000; // actually unused
        public static readonly uint evcReserved_00002000   = 0x00002000; // actually unused
        public static readonly uint evcReserved_00004000   = 0x00004000; // actually unused
        public static readonly uint evcReserved_00008000   = 0x00008000; // actually unused
        // S7 Server Event Code
        public static readonly uint evcPDUincoming         = 0x00010000;
        public static readonly uint evcDataRead            = 0x00020000;
        public static readonly uint evcDataWrite           = 0x00040000;
        public static readonly uint evcNegotiatePDU        = 0x00080000;
        public static readonly uint evcReadSZL             = 0x00100000;
        public static readonly uint evcClock               = 0x00200000;
        public static readonly uint evcUpload              = 0x00400000;
        public static readonly uint evcDownload            = 0x00800000;
        public static readonly uint evcDirectory           = 0x01000000;
        public static readonly uint evcSecurity            = 0x02000000;
        public static readonly uint evcControl             = 0x04000000;
        public static readonly uint evcReserved_08000000   = 0x08000000; // actually unused
        public static readonly uint evcReserved_10000000   = 0x10000000; // actually unused
        public static readonly uint evcReserved_20000000   = 0x20000000; // actually unused
        public static readonly uint evcReserved_40000000   = 0x40000000; // actually unused
        public static readonly uint evcReserved_80000000   = 0x80000000; // actually unused
        // Masks to enable/disable all events
        public static readonly uint evcAll  = 0xFFFFFFFF;
        public static readonly uint evcNone = 0x00000000;
        // Event SubCodes
        public static readonly ushort evsUnknown       = 0x0000;
        public static readonly ushort evsStartUpload   = 0x0001;
        public static readonly ushort evsStartDownload = 0x0001;
        public static readonly ushort evsGetBlockList  = 0x0001;
        public static readonly ushort evsStartListBoT  = 0x0002;
        public static readonly ushort evsListBoT       = 0x0003;
        public static readonly ushort evsGetBlockInfo  = 0x0004;
        public static readonly ushort evsGetClock      = 0x0001;
        public static readonly ushort evsSetClock      = 0x0002;
        public static readonly ushort evsSetPassword   = 0x0001;
        public static readonly ushort evsClrPassword   = 0x0002;
        // Event Params : functions group
        public static readonly ushort grProgrammer     = 0x0041;
        public static readonly ushort grCyclicData     = 0x0042;
        public static readonly ushort grBlocksInfo     = 0x0043;
        public static readonly ushort grSZL            = 0x0044;
        public static readonly ushort grPassword       = 0x0045;
        public static readonly ushort grBSend          = 0x0046;
        public static readonly ushort grClock          = 0x0047;
        public static readonly ushort grSecurity       = 0x0045;
        // Event Params : control codes
        public static readonly ushort CodeControlUnknown   = 0x0000;
        public static readonly ushort CodeControlColdStart = 0x0001;
        public static readonly ushort CodeControlWarmStart = 0x0002;
        public static readonly ushort CodeControlStop      = 0x0003;
        public static readonly ushort CodeControlCompress  = 0x0004;
        public static readonly ushort CodeControlCpyRamRom = 0x0005;
        public static readonly ushort CodeControlInsDel    = 0x0006;
        // Event Result
        public static readonly ushort evrNoError = 0x0000;
        public static readonly ushort evrFragmentRejected  = 0x0001;
        public static readonly ushort evrMalformedPDU      = 0x0002;
        public static readonly ushort evrSparseBytes       = 0x0003;
        public static readonly ushort evrCannotHandlePDU   = 0x0004;
        public static readonly ushort evrNotImplemented    = 0x0005;
        public static readonly ushort evrErrException      = 0x0006;
        public static readonly ushort evrErrAreaNotFound   = 0x0007;
        public static readonly ushort evrErrOutOfRange     = 0x0008;
        public static readonly ushort evrErrOverPDU        = 0x0009;
        public static readonly ushort evrErrTransportSize  = 0x000A;
        public static readonly ushort evrInvalidGroupUData = 0x000B;
        public static readonly ushort evrInvalidSZL        = 0x000C;
        public static readonly ushort evrDataSizeMismatch  = 0x000D;
        public static readonly ushort evrCannotUpload      = 0x000E;
        public static readonly ushort evrCannotDownload    = 0x000F;
        public static readonly ushort evrUploadInvalidID   = 0x0010;
        public static readonly ushort evrResNotFound       = 0x0011;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct USrvEvent
        {
            public IntPtr EvtTime;   // It's platform dependent (32 or 64 bit)
            public Int32  EvtSender;
            public UInt32 EvtCode;
            public ushort EvtRetCode;
            public ushort EvtParam1;
            public ushort EvtParam2;
            public ushort EvtParam3;
            public ushort EvtParam4;
        }

        public struct SrvEvent
        {
            public DateTime EvtTime;
            public Int32  EvtSender;
            public UInt32 EvtCode;
            public ushort EvtRetCode;
            public ushort EvtParam1;
            public ushort EvtParam2;
            public ushort EvtParam3;
            public ushort EvtParam4;
        }

        private IntPtr Server;

        #endregion

        #region [Class Control]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern IntPtr Srv_Create();
        public S7Server()
        {
            Server = Srv_Create();
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_Destroy(ref IntPtr Server);
        ~S7Server()
        {
            Srv_Destroy(ref Server);
        }
        
        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_StartTo(IntPtr Server, [MarshalAs(UnmanagedType.LPStr)] string Address);
        public int StartTo(string Address)
        {
            return Srv_StartTo(Server, Address);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_Start(IntPtr Server);
        public int Start()
        {
            return Srv_Start(Server);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_Stop(IntPtr Server);
        public int Stop()
        {
            return Srv_Stop(Server);
        }

        // Get/SetParam needs a void* parameter, internally it decides the kind of pointer
        // in accord to ParamNumber.
        // To avoid the use of unsafe code we split the DLL functions and use overloaded methods.

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Srv_GetParam")]
        protected static extern int Srv_GetParam_i16(IntPtr Server, Int32 ParamNumber, ref Int16 IntValue);
        public int GetParam(Int32 ParamNumber, ref Int16 IntValue)
        {
            return Srv_GetParam_i16(Server, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Srv_GetParam")]
        protected static extern int Srv_GetParam_u16(IntPtr Server, Int32 ParamNumber, ref UInt16 IntValue);
        public int GetParam(Int32 ParamNumber, ref UInt16 IntValue)
        {
            return Srv_GetParam_u16(Server, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Srv_GetParam")]
        protected static extern int Srv_GetParam_i32(IntPtr Server, Int32 ParamNumber, ref Int32 IntValue);
        public int GetParam(Int32 ParamNumber, ref Int32 IntValue)
        {
            return Srv_GetParam_i32(Server, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Srv_GetParam")]
        protected static extern int Srv_GetParam_u32(IntPtr Server, Int32 ParamNumber, ref UInt32 IntValue);
        public int GetParam(Int32 ParamNumber, ref UInt32 IntValue)
        {
            return Srv_GetParam_u32(Server, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Srv_GetParam")]
        protected static extern int Srv_GetParam_i64(IntPtr Server, Int32 ParamNumber, ref Int64 IntValue);
        public int GetParam(Int32 ParamNumber, ref Int64 IntValue)
        {
            return Srv_GetParam_i64(Server, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Srv_GetParam")]
        protected static extern int Srv_GetParam_u64(IntPtr Server, Int32 ParamNumber, ref UInt64 IntValue);
        public int GetParam(Int32 ParamNumber, ref UInt64 IntValue)
        {
            return Srv_GetParam_u64(Server, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Srv_SetParam")]
        protected static extern int Srv_SetParam_i16(IntPtr Server, Int32 ParamNumber, ref Int16 IntValue);
        public int SetParam(Int32 ParamNumber, ref Int16 IntValue)
        {
            return Srv_SetParam_i16(Server, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Srv_SetParam")]
        protected static extern int Srv_SetParam_u16(IntPtr Server, Int32 ParamNumber, ref UInt16 IntValue);
        public int SetParam(Int32 ParamNumber, ref UInt16 IntValue)
        {
            return Srv_SetParam_u16(Server, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Srv_SetParam")]
        protected static extern int Srv_SetParam_i32(IntPtr Server, Int32 ParamNumber, ref Int32 IntValue);
        public int SetParam(Int32 ParamNumber, ref Int32 IntValue)
        {
            return Srv_SetParam_i32(Server, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Srv_SetParam")]
        protected static extern int Srv_SetParam_u32(IntPtr Server, Int32 ParamNumber, ref UInt32 IntValue);
        public int SetParam(Int32 ParamNumber, ref UInt32 IntValue)
        {
            return Srv_SetParam_u32(Server, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Srv_SetParam")]
        protected static extern int Srv_SetParam_i64(IntPtr Server, Int32 ParamNumber, ref Int64 IntValue);
        public int SetParam(Int32 ParamNumber, ref Int64 IntValue)
        {
            return Srv_SetParam_i64(Server, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Srv_SetParam")]
        protected static extern int Srv_SetParam_u64(IntPtr Server, Int32 ParamNumber, ref UInt64 IntValue);
        public int SetParam(Int32 ParamNumber, ref UInt64 IntValue)
        {
            return Srv_SetParam_u64(Server, ParamNumber, ref IntValue);
        }
        
        #endregion

        #region [Data Areas functions]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_RegisterArea(IntPtr Server, Int32 AreaCode, Int32 Index, byte[] pUsrData, Int32 Size);
        public int RegisterArea(Int32 AreaCode, Int32 Index, byte[] pUsrData, Int32 Size)
        {
            return Srv_RegisterArea(Server, AreaCode, Index, pUsrData, Size);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_UnregisterArea(IntPtr Server, Int32 AreaCode, Int32 Index);
        public int UnregisterArea(Int32 AreaCode, Int32 Index)
        {
            return Srv_UnregisterArea(Server, AreaCode, Index);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_LockArea(IntPtr Server, Int32 AreaCode, Int32 Index);
        public int LockArea(Int32 AreaCode, Int32 Index)
        {
            return Srv_LockArea(Server, AreaCode, Index);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_UnlockArea(IntPtr Server, Int32 AreaCode, Int32 Index);
        public int UnlockArea(Int32 AreaCode, Int32 Index)
        {
            return Srv_UnlockArea(Server, AreaCode, Index);
        }

        #endregion

        #region [Notification functions (Events)]

        public delegate void TSrvCallback(IntPtr usrPtr, ref USrvEvent Event, int Size);

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_SetEventsCallback(IntPtr Server, TSrvCallback Callback, IntPtr usrPtr);
        public int SetEventsCallBack(TSrvCallback Callback, IntPtr usrPtr)
        {
            return Srv_SetEventsCallback(Server, Callback, usrPtr);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_SetReadEventsCallback(IntPtr Server, TSrvCallback Callback, IntPtr usrPtr);
        public int SetReadEventsCallBack(TSrvCallback Callback, IntPtr usrPtr)
        {
            return Srv_SetReadEventsCallback(Server, Callback, usrPtr);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_PickEvent(IntPtr Server, ref USrvEvent Event, ref Int32 EvtReady);
        public bool PickEvent(ref USrvEvent Event)
        {
            Int32 EvtReady = new Int32();
            if (Srv_PickEvent(Server, ref Event, ref EvtReady) == 0)
                return EvtReady != 0;
            else
                return false;
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_ClearEvents(IntPtr Server);
        public int ClearEvents()
        {
            return Srv_ClearEvents(Server);
        }
        
        [DllImport(S7Consts.Snap7LibName, CharSet = CharSet.Ansi)]
        protected static extern int Srv_EventText(ref USrvEvent Event, StringBuilder EvtMsg, int TextSize);
        public string EventText(ref USrvEvent Event)
        {
            StringBuilder Message = new StringBuilder(MsgTextLen);
            Srv_EventText(ref Event, Message, MsgTextLen);
            return Message.ToString();
        }

        public DateTime EvtTimeToDateTime(IntPtr TimeStamp)
        {
            DateTime UnixStartEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return UnixStartEpoch.AddSeconds(Convert.ToDouble(TimeStamp));
        }        
        
        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_GetMask(IntPtr Server, Int32 MaskKind, ref UInt32 Mask);
        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_SetMask(IntPtr Server, Int32 MaskKind, UInt32 Mask);
        
        // Property LogMask R/W
        public UInt32 LogMask {
            get 
            {
                UInt32 Mask = new UInt32();
                if (Srv_GetMask(Server, S7Server.mkLog, ref Mask)==0)
                    return Mask;
                else
                    return 0;
            }
            set
            {
                Srv_SetMask(Server, S7Server.mkLog, value);
            }
        }

        // Property EventMask R/W
        public UInt32 EventMask
        {
            get
            {
                UInt32 Mask = new UInt32();
                if (Srv_GetMask(Server, S7Server.mkEvent, ref Mask) == 0)
                    return Mask;
                else
                    return 0;
            }
            set
            {
                Srv_SetMask(Server, S7Server.mkEvent, value);
            }
        }


        #endregion

        #region [Info functions]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_GetStatus(IntPtr Server, ref Int32 ServerStatus, ref Int32 CpuStatus, ref Int32 ClientsCount);
        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Srv_SetCpuStatus(IntPtr Server, Int32 CpuStatus);       
        
        // Property Virtual CPU status R/W
        public int CpuStatus
        {
            get
            {
                Int32 CStatus = new Int32();
                Int32 SStatus = new Int32();
                Int32 CCount  = new Int32();

                if (Srv_GetStatus(Server, ref CStatus, ref SStatus, ref CCount) == 0)
                    return CStatus;
                else
                    return -1;
            }
            set
            {
                Srv_SetCpuStatus(Server, value);
            }
        }
        
        // Property Server Status Read Only
        public int ServerStatus
        {
            get
            {
                Int32 CStatus = new Int32();
                Int32 SStatus = new Int32();
                Int32 CCount = new Int32();
                if (Srv_GetStatus(Server, ref CStatus, ref SStatus, ref CCount) == 0)
                    return SStatus;
                else
                    return -1;
            }
        }
        
        // Property Clients Count Read Only
        public int ClientsCount
        {
            get
            {
                Int32 CStatus = new Int32();
                Int32 SStatus = new Int32();
                Int32 CCount = new Int32();
                if (Srv_GetStatus(Server, ref CStatus, ref SStatus, ref CCount) == 0)
                    return CCount;
                else
                    return -1;
            }
        }
               
        [DllImport(S7Consts.Snap7LibName, CharSet = CharSet.Ansi)]
        protected static extern int Srv_ErrorText(int Error, StringBuilder ErrMsg, int TextSize);
        public string ErrorText(int Error)
        {
            StringBuilder Message = new StringBuilder(MsgTextLen);
            Srv_ErrorText(Error, Message, MsgTextLen);
            return Message.ToString();
        }
                
        #endregion
    }

    public class S7Partner
    {
        #region [Constants, private vars and TypeDefs]

        private const int MsgTextLen = 1024;
        
        // Status
        public static readonly int par_stopped    = 0;   // stopped
        public static readonly int par_connecting = 1;   // running and active connecting
        public static readonly int par_waiting    = 2;   // running and waiting for a connection
        public static readonly int par_linked     = 3;   // running and connected : linked
        public static readonly int par_sending    = 4;   // sending data
        public static readonly int par_receiving  = 5;   // receiving data
        public static readonly int par_binderror  = 6;   // error starting passive server

        // Errors
        public static readonly uint errParAddressInUse       = 0x00200000;
        public static readonly uint errParNoRoom             = 0x00300000;
        public static readonly uint errServerNoRoom          = 0x00400000;
        public static readonly uint errParInvalidParams      = 0x00500000;
        public static readonly uint errParNotLinked          = 0x00600000;
        public static readonly uint errParBusy               = 0x00700000;
        public static readonly uint errParFrameTimeout       = 0x00800000;
        public static readonly uint errParInvalidPDU         = 0x00900000;
        public static readonly uint errParSendTimeout        = 0x00A00000;
        public static readonly uint errParRecvTimeout        = 0x00B00000;
        public static readonly uint errParSendRefused        = 0x00C00000;
        public static readonly uint errParNegotiatingPDU     = 0x00D00000;
        public static readonly uint errParSendingBlock       = 0x00E00000;
        public static readonly uint errParRecvingBlock       = 0x00F00000;
        public static readonly uint errBindError             = 0x01000000;
        public static readonly uint errParDestroying         = 0x01100000;
        public static readonly uint errParInvalidParamNumber = 0x01200000; 
        public static readonly uint errParCannotChangeParam  = 0x01300000; 
     
        // Generic byte buffer structure, you may need to declare a more
        // specialistic one in your program.
        // It's used to cast the input pointer that cames from the callback.
        // See the passive partned demo and the delegate S7ParRecvCallback.

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct S7Buffer
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x8000)]
            public byte[] Data;
        }

        // Job status
        private const int JobComplete = 0;
        private const int JobPending  = 1;

        private IntPtr Partner;

        private Int32 parBytesSent;
        private Int32 parBytesRecv;
	    private Int32 parSendErrors;
        private Int32 parRecvErrors;

        #endregion

        #region [Class Control]
        
        [DllImport(S7Consts.Snap7LibName)]
        protected static extern IntPtr Par_Create(Int32 ParActive);
        public S7Partner(int Active)
        {
            Partner= Par_Create(Active);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Par_Destroy(ref IntPtr Partner);
        ~S7Partner()
        {
            Par_Destroy(ref Partner);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Par_StartTo(
            IntPtr Partner,
            [MarshalAs(UnmanagedType.LPStr)] string LocalAddress,
            [MarshalAs(UnmanagedType.LPStr)] string RemoteAddress,
            UInt16 LocalTSAP,
            UInt16 RemoteTSAP);

        public int StartTo(
            string LocalAddress,
            string RemoteAddress,
            UInt16 LocalTSAP,
            UInt16 RemoteTSAP)
        {
            return Par_StartTo(Partner, LocalAddress, RemoteAddress, LocalTSAP, RemoteTSAP);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Par_Start(IntPtr Partner);
        public int Start()
        {
            return Par_Start(Partner);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Par_Stop(IntPtr Partner);
        public int Stop()
        {
            return Par_Stop(Partner);
        }

        // Get/SetParam needs a void* parameter, internally it decides the kind of pointer
        // in accord to ParamNumber.
        // To avoid the use of unsafe code we split the DLL functions and use overloaded methods.

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Par_GetParam")]
        protected static extern int Par_GetParam_i16(IntPtr Partner, Int32 ParamNumber, ref Int16 IntValue);
        public int GetParam(Int32 ParamNumber, ref Int16 IntValue)
        {
            return Par_GetParam_i16(Partner, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Par_GetParam")]
        protected static extern int Par_GetParam_u16(IntPtr Partner, Int32 ParamNumber, ref UInt16 IntValue);
        public int GetParam(Int32 ParamNumber, ref UInt16 IntValue)
        {
            return Par_GetParam_u16(Partner, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Par_GetParam")]
        protected static extern int Par_GetParam_i32(IntPtr Partner, Int32 ParamNumber, ref Int32 IntValue);
        public int GetParam(Int32 ParamNumber, ref Int32 IntValue)
        {
            return Par_GetParam_i32(Partner, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Par_GetParam")]
        protected static extern int Par_GetParam_u32(IntPtr Partner, Int32 ParamNumber, ref UInt32 IntValue);
        public int GetParam(Int32 ParamNumber, ref UInt32 IntValue)
        {
            return Par_GetParam_u32(Partner, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Par_GetParam")]
        protected static extern int Par_GetParam_i64(IntPtr Partner, Int32 ParamNumber, ref Int64 IntValue);
        public int GetParam(Int32 ParamNumber, ref Int64 IntValue)
        {
            return Par_GetParam_i64(Partner, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Par_GetParam")]
        protected static extern int Par_GetParam_u64(IntPtr Partner, Int32 ParamNumber, ref UInt64 IntValue);
        public int GetParam(Int32 ParamNumber, ref UInt64 IntValue)
        {
            return Par_GetParam_u64(Partner, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Par_SetParam")]
        protected static extern int Par_SetParam_i16(IntPtr Partner, Int32 ParamNumber, ref Int16 IntValue);
        public int SetParam(Int32 ParamNumber, ref Int16 IntValue)
        {
            return Par_SetParam_i16(Partner, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Par_SetParam")]
        protected static extern int Par_SetParam_u16(IntPtr Partner, Int32 ParamNumber, ref UInt16 IntValue);
        public int SetParam(Int32 ParamNumber, ref UInt16 IntValue)
        {
            return Par_SetParam_u16(Partner, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Par_SetParam")]
        protected static extern int Par_SetParam_i32(IntPtr Partner, Int32 ParamNumber, ref Int32 IntValue);
        public int SetParam(Int32 ParamNumber, ref Int32 IntValue)
        {
            return Par_SetParam_i32(Partner, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Par_SetParam")]
        protected static extern int Par_SetParam_u32(IntPtr Partner, Int32 ParamNumber, ref UInt32 IntValue);
        public int SetParam(Int32 ParamNumber, ref UInt32 IntValue)
        {
            return Par_SetParam_u32(Partner, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Par_SetParam")]
        protected static extern int Par_SetParam_i64(IntPtr Partner, Int32 ParamNumber, ref Int64 IntValue);
        public int SetParam(Int32 ParamNumber, ref Int64 IntValue)
        {
            return Par_SetParam_i64(Partner, ParamNumber, ref IntValue);
        }

        [DllImport(S7Consts.Snap7LibName, EntryPoint = "Par_SetParam")]
        protected static extern int Par_SetParam_u64(IntPtr Partner, Int32 ParamNumber, ref UInt64 IntValue);
        public int SetParam(Int32 ParamNumber, ref UInt64 IntValue)
        {
            return Par_SetParam_u64(Partner, ParamNumber, ref IntValue);
        }

        #endregion

        #region [Data I/O functions : BSend]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Par_BSend(IntPtr Partner, UInt32 R_ID, byte[] Buffer, Int32 Size);
        public int BSend(UInt32 R_ID, byte[] Buffer, Int32 Size)
        {
            return Par_BSend(Partner, R_ID, Buffer, Size);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Par_AsBSend(IntPtr Partner, UInt32 R_ID, byte[] Buffer, Int32 Size);
        public int AsBSend(UInt32 R_ID, byte[] Buffer, Int32 Size)
        {
            return Par_AsBSend(Partner, R_ID, Buffer, Size);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Par_CheckAsBSendCompletion(IntPtr Partner, ref Int32 opResult);
        public bool CheckAsBSendCompletion(ref int opResult)
        {
            return Par_CheckAsBSendCompletion(Partner, ref opResult)==JobComplete;
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Par_WaitAsBSendCompletion(IntPtr Partner, Int32 Timeout);
        public int WaitAsBSendCompletion(int Timeout)
        {
            return Par_WaitAsBSendCompletion(Partner, Timeout);
        }

        public delegate void S7ParSendCompletion(IntPtr usrPtr, int opResult);

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Par_SetSendCallback(IntPtr Partner, S7ParSendCompletion Completion, IntPtr usrPtr);
        public int SetSendCallBack(S7ParSendCompletion Completion, IntPtr usrPtr)
        {
            return Par_SetSendCallback(Partner, Completion, usrPtr);
        }

        #endregion

        #region [Data I/O functions : BRecv]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Par_BRecv(IntPtr Partner, ref UInt32 R_ID, byte[] Buffer, ref Int32 Size, UInt32 Timeout);
        public int BRecv(ref UInt32 R_ID, byte[] Buffer, ref Int32 Size, UInt32 Timeout)
        {
            return Par_BRecv(Partner, ref R_ID, Buffer, ref Size, Timeout);
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Par_CheckAsBRecvCompletion(IntPtr Partner, ref Int32 opResult, ref UInt32 R_ID, byte[] Buffer, ref Int32 Size);
        public bool CheckAsBRecvCompletion(ref Int32 opResult, ref UInt32 R_ID, byte[] Buffer, ref Int32 Size)
        {
            Par_CheckAsBRecvCompletion(Partner, ref opResult, ref R_ID, Buffer, ref Size);
            return opResult == JobComplete;
        }

        public delegate void S7ParRecvCallback(IntPtr usrPtr, int opResult, uint R_ID, IntPtr pData, int Size);

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Par_SetRecvCallback(IntPtr Partner, S7ParRecvCallback Callback, IntPtr usrPtr);
        public int SetRecvCallback(S7ParRecvCallback Callback, IntPtr usrPtr)
        {
            return Par_SetRecvCallback(Partner, Callback, usrPtr);
        }

        #endregion

        #region [Info functions]

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Par_GetLastError(IntPtr Partner, ref Int32 LastError);
        public int LastError(ref Int32 LastError)
        {
            Int32 PartnerLastError = new Int32();
            if (Par_GetLastError(Partner, ref PartnerLastError) == 0)
                return (int)PartnerLastError;
            else
                return -1;
        }

        [DllImport(S7Consts.Snap7LibName, CharSet = CharSet.Ansi)]
        protected static extern int Par_ErrorText(int Error, StringBuilder ErrMsg, int TextSize);
        public string ErrorText(int Error)
        {
            StringBuilder Message = new StringBuilder(MsgTextLen);
            Par_ErrorText(Error, Message, MsgTextLen);
            return Message.ToString();
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Par_GetStats(IntPtr Partner, ref Int32 BytesSent, ref Int32 BytesRecv,
	       ref Int32 SendErrors, ref Int32 RecvErrors);
        
        private void GetStatistics()
        {
            if (Par_GetStats(Partner, ref parBytesSent, ref parBytesRecv, ref parSendErrors, ref parRecvErrors) != 0)
            {
                parBytesSent = -1;
                parBytesRecv = -1;
                parSendErrors = -1;
                parRecvErrors = -1;           
            }        
        }

        public int BytesSent
        {
            get
            {
                GetStatistics();
                return parBytesSent;
            }
        }

        public int BytesRecv
        {
            get
            {
                GetStatistics();
                return parBytesRecv;
            }
        }

        public int SendErrors
        {
            get
            {
                GetStatistics();
                return parSendErrors;
            }
        }

        public int RecvErrors
        {
            get
            {
                GetStatistics();
                return parRecvErrors;
            }
        }

        [DllImport(S7Consts.Snap7LibName)]
        protected static extern int Par_GetStatus(IntPtr Partner, ref Int32 Status);

        public int Status
        {
            get
            {
                int ParStatus = new int();
                if (Par_GetStatus(Partner, ref ParStatus) != 0)
                    return -1;
                else
                    return ParStatus;
            }             
        }
        // simply useful
        public bool Linked
        {
            get
            {
                return Status == par_linked;
            }
        }
        #endregion

    }
}
