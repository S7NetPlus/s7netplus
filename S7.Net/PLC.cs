using System;
using System.Net.Sockets;


namespace S7.Net
{
    /// <summary>
    /// Creates an instance of S7.Net driver
    /// </summary>
    public partial class Plc : IDisposable
    {
        private const int CONNECTION_TIMED_OUT_ERROR_CODE = 10060;
        
        //TCP connection to device
        private TcpClient tcpClient;
        private NetworkStream stream;

        /// <summary>
        /// IP address of the PLC
        /// </summary>
        public string IP { get; private set; }

        /// <summary>
        /// CPU type of the PLC
        /// </summary>
        public CpuType CPU { get; private set; }

        /// <summary>
        /// Rack of the PLC
        /// </summary>
        public Int16 Rack { get; private set; }

        /// <summary>
        /// Slot of the CPU of the PLC
        /// </summary>
        public Int16 Slot { get; private set; }

        /// <summary>
        /// Max PDU size this cpu supports
        /// </summary>
        public Int16 MaxPDUSize { get; set; }
        
        /// <summary>
        /// Returns true if a connection to the PLC can be established
        /// </summary>
        public bool IsAvailable
        {
            //TODO: Fix This
            get
            {
                    return Connect() == ErrorCode.NoError;                   
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
        /// Contains the last error registered when executing a function
        /// </summary>
        public string LastErrorString { get; private set; }

        /// <summary>
        /// Contains the last error code registered when executing a function
        /// </summary>
        public ErrorCode LastErrorCode { get; private set; }
        
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
