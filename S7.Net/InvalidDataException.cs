using System;

namespace S7.Net
{
    #if NET_FULL
    [Serializable]
    #endif
    public class InvalidDataException : Exception
    {
        public byte[] ReceivedData { get; }
        public int ErrorIndex { get; }
        public byte ExpectedValue { get; }

        public InvalidDataException(string message, byte[] receivedData, int errorIndex, byte expectedValue)
            : base(FormatMessage(message, receivedData, errorIndex, expectedValue))
        {
            ReceivedData = receivedData;
            ErrorIndex = errorIndex;
            ExpectedValue = expectedValue;
        }

        #if NET_FULL
        protected InvalidDataException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            ReceivedData = (byte[]) info.GetValue(nameof(ReceivedData), typeof(byte[]));
            ErrorIndex = info.GetInt32(nameof(ErrorIndex));
            ExpectedValue = info.GetByte(nameof(ExpectedValue));
        }
        #endif

        private static string FormatMessage(string message, byte[] receivedData, int errorIndex, byte expectedValue)
        {
            if (errorIndex >= receivedData.Length)
                throw new ArgumentOutOfRangeException(nameof(errorIndex),
                    $"{nameof(errorIndex)} {errorIndex} is outside the bounds of {nameof(receivedData)} with length {receivedData.Length}.");

            return $"{message} Invalid data received. Expected '{expectedValue}' at index {errorIndex}, " +
                $"but received {receivedData[errorIndex]}. See the {nameof(ReceivedData)} property " +
                "for the full message received.";
        }
    }
}