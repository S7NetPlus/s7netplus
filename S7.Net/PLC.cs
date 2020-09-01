using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using S7.Net.Types;


namespace S7.Net
{
    /// <summary>
    /// Creates an instance of S7.Net driver
    /// </summary>
    public partial class Plc : IDisposable
    {
        private const int CONNECTION_TIMED_OUT_ERROR_CODE = 10060;
        
        //TCP connection to device
        private TcpClient? tcpClient;
        private NetworkStream? _stream;

        private int readTimeout = System.Threading.Timeout.Infinite;
        private int writeTimeout = System.Threading.Timeout.Infinite;

        /// <summary>
        /// IP address of the PLC
        /// </summary>
        public string IP { get; }

        /// <summary>
        /// PORT Number of the PLC, default is 102
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// CPU type of the PLC
        /// </summary>
        public CpuType CPU { get; }

        /// <summary>
        /// Rack of the PLC
        /// </summary>
        public Int16 Rack { get; }

        /// <summary>
        /// Slot of the CPU of the PLC
        /// </summary>
        public Int16 Slot { get; }

        /// <summary>
        /// Max PDU size this cpu supports
        /// </summary>
        public Int16 MaxPDUSize { get; set; }

        /// <summary>Gets or sets the amount of time that a read operation blocks waiting for data from PLC.</summary>
        /// <returns>A <see cref="T:System.Int32" /> that specifies the amount of time, in milliseconds, that will elapse before a read operation fails. The default value, <see cref="F:System.Threading.Timeout.Infinite" />, specifies that the read operation does not time out.</returns>
        public int ReadTimeout
        {
            get => readTimeout;
            set
            {
                readTimeout = value;
                if (tcpClient != null) tcpClient.ReceiveTimeout = readTimeout;
            }
        }

        /// <summary>Gets or sets the amount of time that a write operation blocks waiting for data to PLC. </summary>
        /// <returns>A <see cref="T:System.Int32" /> that specifies the amount of time, in milliseconds, that will elapse before a write operation fails. The default value, <see cref="F:System.Threading.Timeout.Infinite" />, specifies that the write operation does not time out.</returns>
        public int WriteTimeout
        {
            get => writeTimeout;
            set
            {
                writeTimeout = value;
                if (tcpClient != null) tcpClient.SendTimeout = writeTimeout;
            }
        }
        
        /// <summary>
        /// Returns true if a connection to the PLC can be established
        /// </summary>
        public bool IsAvailable
        {
            //TODO: Fix This
            get
            {
                try
                {
                    Connect();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Checks if the socket is connected and polls the other peer (the PLC) to see if it's connected.
        /// This is the variable that you should continously check to see if the communication is working
        /// See also: http://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
        /// </summary>
        public bool IsConnected
        {
            get
            {
                try
                {
                    if (tcpClient == null)
                        return false;

                    //TODO: Actually check communication by sending an empty TPDU
                    return tcpClient.Connected;
                }
                catch { return false; }
            }
        }

        /// <summary>
        /// Creates a PLC object with all the parameters needed for connections.
        /// For S7-1200 and S7-1500, the default is rack = 0 and slot = 0.
        /// You need slot > 0 if you are connecting to external ethernet card (CP).
        /// For S7-300 and S7-400 the default is rack = 0 and slot = 2.
        /// </summary>
        /// <param name="cpu">CpuType of the PLC (select from the enum)</param>
        /// <param name="ip">Ip address of the PLC</param>
        /// <param name="port">Port address of the PLC, default 102</param>
        /// <param name="rack">rack of the PLC, usually it's 0, but check in the hardware configuration of Step7 or TIA portal</param>
        /// <param name="slot">slot of the CPU of the PLC, usually it's 2 for S7300-S7400, 0 for S7-1200 and S7-1500.
        ///  If you use an external ethernet card, this must be set accordingly.</param>
        public Plc(CpuType cpu, string ip, int port, Int16 rack, Int16 slot)
        {
            if (!Enum.IsDefined(typeof(CpuType), cpu))
                throw new ArgumentException($"The value of argument '{nameof(cpu)}' ({cpu}) is invalid for Enum type '{typeof(CpuType).Name}'.", nameof(cpu));

            if (string.IsNullOrEmpty(ip))
                throw new ArgumentException("IP address must valid.", nameof(ip));

            CPU = cpu;
            IP = ip;
            Port = port;
            Rack = rack;
            Slot = slot;
            MaxPDUSize = 240;
        }
        /// <summary>
        /// Creates a PLC object with all the parameters needed for connections.
        /// For S7-1200 and S7-1500, the default is rack = 0 and slot = 0.
        /// You need slot > 0 if you are connecting to external ethernet card (CP).
        /// For S7-300 and S7-400 the default is rack = 0 and slot = 2.
        /// </summary>
        /// <param name="cpu">CpuType of the PLC (select from the enum)</param>
        /// <param name="ip">Ip address of the PLC</param>
        /// <param name="rack">rack of the PLC, usually it's 0, but check in the hardware configuration of Step7 or TIA portal</param>
        /// <param name="slot">slot of the CPU of the PLC, usually it's 2 for S7300-S7400, 0 for S7-1200 and S7-1500.
        ///  If you use an external ethernet card, this must be set accordingly.</param>
        public Plc(CpuType cpu, string ip, Int16 rack, Int16 slot)
        {
            if (!Enum.IsDefined(typeof(CpuType), cpu))
                throw new ArgumentException($"The value of argument '{nameof(cpu)}' ({cpu}) is invalid for Enum type '{typeof(CpuType).Name}'.", nameof(cpu));

            if (string.IsNullOrEmpty(ip))
                throw new ArgumentException("IP address must valid.", nameof(ip));

            CPU = cpu;
            IP = ip;
            Port = 102;
            Rack = rack;
            Slot = slot;
            MaxPDUSize = 240;
        }

        /// <summary>
        /// Close connection to PLC
        /// </summary>
        public void Close()
        {
            if (tcpClient != null)
            {
                if (tcpClient.Connected) tcpClient.Close();
            }
        }

        private void AssertPduSizeForRead(ICollection<DataItem> dataItems)
        {
            // 12 bytes of header data, 12 bytes of parameter data for each dataItem
            if ((dataItems.Count + 1) * 12 > MaxPDUSize) throw new Exception("Too many vars requested for read");
            
            // 14 bytes of header data, 4 bytes of result data for each dataItem and the actual data
            if (GetDataLength(dataItems) + dataItems.Count * 4 + 14 > MaxPDUSize) throw new Exception("Too much data requested for read");
        }

        private void AssertPduSizeForWrite(ICollection<DataItem> dataItems)
        {
            // 12 bytes of header data, 18 bytes of parameter data for each dataItem
            if (dataItems.Count * 18 + 12 > MaxPDUSize) throw new Exception("Too many vars supplied for write");

            // 12 bytes of header data, 16 bytes of data for each dataItem and the actual data
            if (GetDataLength(dataItems) + dataItems.Count * 16 + 12 > MaxPDUSize)
                throw new Exception("Too much data supplied for write");
        }

        private void ConfigureConnection()
        {
            if (tcpClient == null)
            {
                return;
            }

            tcpClient.ReceiveTimeout = ReadTimeout;
            tcpClient.SendTimeout = WriteTimeout;
        }

        private int GetDataLength(IEnumerable<DataItem> dataItems)
        {
            // Odd length variables are 0-padded
            return dataItems.Select(di => VarTypeToByteLength(di.VarType, di.Count))
                .Sum(len => (len & 1) == 1 ? len + 1 : len);
        }

        private static void AssertReadResponse(byte[] s7Data, int dataLength)
        {
            var expectedLength = dataLength + 18;

            PlcException NotEnoughBytes() =>
                new PlcException(ErrorCode.WrongNumberReceivedBytes,
                    $"Received {s7Data.Length} bytes: '{BitConverter.ToString(s7Data)}', expected {expectedLength} bytes.")
            ;

            if (s7Data == null)
                throw new PlcException(ErrorCode.WrongNumberReceivedBytes, "No s7Data received.");

            if (s7Data.Length < 15) throw NotEnoughBytes();

            if (s7Data[14] != 0xff)
                throw new PlcException(ErrorCode.ReadData,
                    $"Invalid response from PLC: '{BitConverter.ToString(s7Data)}'.");

            if (s7Data.Length < expectedLength) throw NotEnoughBytes();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Dispose Plc Object
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Plc() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
