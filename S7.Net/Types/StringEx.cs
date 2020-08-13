﻿using System;
using System.Text;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the methods to convert from S7 strings to C# strings
    ///   there are two kinds how strings a send. This one is with a pre of two bytes
    ///   they contain the length of the string
    /// </summary>
    public static class StringEx
    {
        /// <summary>
        /// Converts S7 bytes to a string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string FromByteArray(byte[] bytes)
        {
            if (bytes.Length < 2) return "";

            int size = bytes[0];
            int length = bytes[1];

            try
            {
                return Encoding.ASCII.GetString(bytes, 2, length);
            }
            catch (Exception e)
            {
                throw new PlcException(ErrorCode.ReadData,
                    $"Failed to parse {VarType.StringEx} from data. Following fields were read: size: '{size}', actual length: '{length}', total number of bytes (including header): '{bytes.Length}'.",
                    e);
            }
            
        }

        /// <summary>
        /// Converts a <see cref="T:string"/> to S7 string with 2-byte header.
        /// </summary>
        /// <param name="value">The string to convert to byte array.</param>
        /// <param name="reservedLength">The length (in bytes) allocated in PLC for string excluding header.</param>
        /// <returns>A <see cref="T:byte[]" /> containing the string header and string value with a maximum length of <paramref name="reservedLength"/> + 2.</returns>
        public static byte[] ToByteArray(string value, int reservedLength)
        {
            if (reservedLength > byte.MaxValue) throw new ArgumentException($"The maximum string length supported is {byte.MaxValue}.");

            var length = value?.Length;
            if (length > reservedLength) length = reservedLength;

            var bytes = new byte[(length ?? 0) + 2];
            bytes[0] = (byte) reservedLength;

            if (value == null) return bytes;

            bytes[1] = (byte) Encoding.ASCII.GetBytes(value, 0, length.Value, bytes, 2);
            return bytes;
        }
    }
}
