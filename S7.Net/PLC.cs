using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using S7.Net.Internal;
using S7.Net.Protocol;
using S7.Net.Types;


namespace S7.Net
{
    /// <summary>
    /// Creates an instance of S7.Net driver
    /// </summary>
    public partial class Plc : IDisposable
    {
        /// <summary>
        /// The default port for the S7 protocol.
        /// </summary>
        public const int DefaultPort = 102;

        /// <summary>
        /// The default timeout (in milliseconds) used for <see cref="P:ReadTimeout"/> and <see cref="P:WriteTimeout"/>.
        /// </summary>
        public const int DefaultTimeout = 10_000;

        private readonly TaskQueue queue = new TaskQueue();

        //TCP connection to device
        private TcpClient? tcpClient;
        private NetworkStream? _stream;

        private int readTimeout = DefaultTimeout; // default no timeout
        private int writeTimeout = DefaultTimeout; // default no timeout

        /// <summary>
        /// IP address of the PLC
        /// </summary>
        public string IP { get; }

        /// <summary>
        /// PORT Number of the PLC, default is 102
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// The TSAP addresses used during the connection request.
        /// </summary>
        public TsapPair TsapPair { get; set; }

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
        public int MaxPDUSize { get; private set; }

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
        /// Gets a value indicating whether a connection to the PLC has been established.
        /// </summary>
        /// <remarks>
        /// The <see cref="IsConnected"/> property gets the connection state of the Client socket as
        /// of the last I/O operation. When it returns <c>false</c>, the Client socket was either
        /// never  connected, or is no longer connected.
        ///
        /// <para>
        /// Because the <see cref="IsConnected"/> property only reflects the state of the connection
        /// as of the most recent operation, you should attempt to send or receive a message to
        /// determine the current state. After the message send fails, this property no longer
        /// returns <c>true</c>. Note that this behavior is by design. You cannot reliably test the
        /// state of the connection because, in the time between the test and a send/receive, the
        /// connection could have been lost. Your code should assume the socket is connected, and
        /// gracefully handle failed transmissions.
        /// </para>
        /// </remarks>
        public bool IsConnected => tcpClient?.Connected ?? false;

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
            : this(cpu, ip, DefaultPort, rack, slot)
        {
        }

        /// <summary>
        /// Creates a PLC object with all the parameters needed for connections.
        /// For S7-1200 and S7-1500, the default is rack = 0 and slot = 0.
        /// You need slot > 0 if you are connecting to external ethernet card (CP).
        /// For S7-300 and S7-400 the default is rack = 0 and slot = 2.
        /// </summary>
        /// <param name="cpu">CpuType of the PLC (select from the enum)</param>
        /// <param name="ip">Ip address of the PLC</param>
        /// <param name="port">Port number used for the connection, default 102.</param>
        /// <param name="rack">rack of the PLC, usually it's 0, but check in the hardware configuration of Step7 or TIA portal</param>
        /// <param name="slot">slot of the CPU of the PLC, usually it's 2 for S7300-S7400, 0 for S7-1200 and S7-1500.
        ///  If you use an external ethernet card, this must be set accordingly.</param>
        public Plc(CpuType cpu, string ip, int port, Int16 rack, Int16 slot)
            : this(ip, port, TsapPair.GetDefaultTsapPair(cpu, rack, slot))
        {
            if (!Enum.IsDefined(typeof(CpuType), cpu))
                throw new ArgumentException(
                    $"The value of argument '{nameof(cpu)}' ({cpu}) is invalid for Enum type '{typeof(CpuType).Name}'.",
                    nameof(cpu));

            CPU = cpu;
            Rack = rack;
            Slot = slot;
        }

        /// <summary>
        /// Creates a PLC object with all the parameters needed for connections.
        /// For S7-1200 and S7-1500, the default is rack = 0 and slot = 0.
        /// You need slot > 0 if you are connecting to external ethernet card (CP).
        /// For S7-300 and S7-400 the default is rack = 0 and slot = 2.
        /// </summary>
        /// <param name="ip">Ip address of the PLC</param>
        /// <param name="tsapPair">The TSAP addresses used for the connection request.</param>
        public Plc(string ip, TsapPair tsapPair) : this(ip, DefaultPort, tsapPair)
        {
        }

        /// <summary>
        /// Creates a PLC object with all the parameters needed for connections. Use this constructor
        /// if you want to manually override the TSAP addresses used during the connection request.
        /// </summary>
        /// <param name="ip">Ip address of the PLC</param>
        /// <param name="port">Port number used for the connection, default 102.</param>
        /// <param name="tsapPair">The TSAP addresses used for the connection request.</param>
        public Plc(string ip, int port, TsapPair tsapPair)
        {
            if (string.IsNullOrEmpty(ip))
                throw new ArgumentException("IP address must valid.", nameof(ip));

            IP = ip;
            Port = port;
            MaxPDUSize = 240;
            TsapPair = tsapPair;
        }

        /// <summary>
        /// Close connection to PLC
        /// </summary>
        public void Close()
        {
            if (tcpClient != null)
            {
                if (tcpClient.Connected) tcpClient.Close();
                tcpClient = null; // Can not reuse TcpClient once connection gets closed.
            }
        }

        private void AssertPduSizeForRead(ICollection<DataItem> dataItems)
        {
            // send request limit: 19 bytes of header data, 12 bytes of parameter data for each dataItem
            var requiredRequestSize = 19 + dataItems.Count * 12;
            if (requiredRequestSize > MaxPDUSize) throw new Exception($"Too many vars requested for read. Request size ({requiredRequestSize}) is larger than protocol limit ({MaxPDUSize}).");

            // response limit: 14 bytes of header data, 4 bytes of result data for each dataItem and the actual data
            var requiredResponseSize = GetDataLength(dataItems) + dataItems.Count * 4 + 14;
            if (requiredResponseSize > MaxPDUSize) throw new Exception($"Too much data requested for read. Response size ({requiredResponseSize}) is larger than protocol limit ({MaxPDUSize}).");
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

            ValidateResponseCode((ReadWriteErrorCode)s7Data[14]);

            if (s7Data.Length < expectedLength) throw NotEnoughBytes();
        }

        internal static void ValidateResponseCode(ReadWriteErrorCode statusCode)
        {
            switch (statusCode)
            {
                case ReadWriteErrorCode.ObjectDoesNotExist:
                    throw new Exception("Received error from PLC: Object does not exist.");
                case ReadWriteErrorCode.DataTypeInconsistent:
                    throw new Exception("Received error from PLC: Data type inconsistent.");
                case ReadWriteErrorCode.DataTypeNotSupported:
                    throw new Exception("Received error from PLC: Data type not supported.");
                case ReadWriteErrorCode.AccessingObjectNotAllowed:
                    throw new Exception("Received error from PLC: Accessing object not allowed.");
                case ReadWriteErrorCode.AddressOutOfRange:
                    throw new Exception("Received error from PLC: Address out of range.");
                case ReadWriteErrorCode.HardwareFault:
                    throw new Exception("Received error from PLC: Hardware fault.");
                case ReadWriteErrorCode.Success:
                    break;
                default:
                    throw new Exception( $"Invalid response from PLC: statusCode={(byte)statusCode}.");
            }
        }

        private Stream GetStreamIfAvailable()
        {
            if (_stream == null)
            {
                throw new PlcException(ErrorCode.ConnectionError, "Plc is not connected");
            }

            return _stream;
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
