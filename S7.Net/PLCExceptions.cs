using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace S7.Net
{
    internal class WrongNumberOfBytesException : Exception
    {
        public WrongNumberOfBytesException() : base()
        {
        }

        public WrongNumberOfBytesException(string message) : base(message)
        {
        }

        public WrongNumberOfBytesException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WrongNumberOfBytesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    internal class InvalidAddressException : Exception
    {
        public InvalidAddressException() : base ()
        {
        }

        public InvalidAddressException(string message) : base(message)
        {
        }

        public InvalidAddressException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidAddressException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    internal class InvalidVariableTypeException : Exception
    {
        public InvalidVariableTypeException() : base()
        {
        }

        public InvalidVariableTypeException(string message) : base(message)
        {
        }

        public InvalidVariableTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidVariableTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    internal class TPKTInvalidException : Exception
    {
        public TPKTInvalidException() : base()
        {
        }

        public TPKTInvalidException(string message) : base(message)
        {
        }

        public TPKTInvalidException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TPKTInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
