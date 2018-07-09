using System;

namespace S7.Net
{
    #if NET_FULL
    [Serializable]
    #endif
    public class PlcException : Exception
    {
        public ErrorCode ErrorCode { get; }

        public PlcException(ErrorCode errorCode) : this(errorCode, $"PLC communication failed with error '{errorCode}'.")
        {
        }

        public PlcException(ErrorCode errorCode, Exception innerException) : this(errorCode, innerException.Message,
            innerException)
        {
        }

        public PlcException(ErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public PlcException(ErrorCode errorCode, string message, Exception inner) : base(message, inner)
        {
            ErrorCode = errorCode;
        }

        #if NET_FULL
        protected PlcException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            ErrorCode = (ErrorCode) info.GetInt32(nameof(ErrorCode));
        }
        #endif
    }
}