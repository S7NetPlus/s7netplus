using System;
using System.Text;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the methods to convert from S7 wstrings to C# strings
    /// An S7 WString has a preceding 4 byte header containing its capacity and length
    /// </summary>
    public static class S7WString
    {
        /// <summary>
        /// Converts S7 bytes to a string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string FromByteArray(byte[] bytes)
        {
            if (bytes.Length < 4)
            {
                throw new PlcException(ErrorCode.ReadData, "Malformed S7 WString / too short");
            }

            int size = (bytes[0] << 8) | bytes[1];
            int length = (bytes[2] << 8) | bytes[3];

            if (length > size)
            {
                throw new PlcException(ErrorCode.ReadData, "Malformed S7 WString / length larger than capacity");
            }

            try
            {
                return Encoding.BigEndianUnicode.GetString(bytes, 4, length * 2);
            }
            catch (Exception e)
            {
                throw new PlcException(ErrorCode.ReadData,
                    $"Failed to parse {VarType.S7WString} from data. Following fields were read: size: '{size}', actual length: '{length}', total number of bytes (including header): '{bytes.Length}'.",
                    e);
            }
            
        }

        /// <summary>
        /// Converts a <see cref="T:string"/> to S7 wstring with 4-byte header.
        /// </summary>
        /// <param name="value">The string to convert to byte array.</param>
        /// <param name="reservedLength">The length (in characters) allocated in PLC for the string.</param>
        /// <returns>A <see cref="T:byte[]" /> containing the string header and string value with a maximum length of <paramref name="reservedLength"/> + 4.</returns>
        public static byte[] ToByteArray(string? value, int reservedLength)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (reservedLength > 16382) throw new ArgumentException("The maximum string length supported is 16382.");
            
            var buffer = new byte[4 + reservedLength * 2];
            buffer[0] = (byte)((reservedLength >> 8) & 0xFF);
            buffer[1] = (byte)(reservedLength & 0xFF);
            buffer[2] = (byte)((value.Length >> 8) & 0xFF);
            buffer[3] = (byte)(value.Length & 0xFF);

            var stringLength = Encoding.BigEndianUnicode.GetBytes(value, 0, value.Length, buffer, 4) / 2;
            if (stringLength > reservedLength) throw new ArgumentException($"The provided string length ({stringLength} is larger than the specified reserved length ({reservedLength}).");

            return buffer;
        }
    }
}
