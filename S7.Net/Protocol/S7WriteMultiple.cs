using System;
using System.Collections.Generic;
using S7.Net.Types;

namespace S7.Net.Protocol
{
    internal static class S7WriteMultiple
    {
        public static int CreateRequest(ByteArray message, DataItem[] dataItems)
        {
            message.Add(Header.Template);

            message[Header.Offsets.ParameterCount] = (byte) dataItems.Length;
            var paramSize = dataItems.Length * Parameter.Template.Length;

            Serialization.SetWordAt(message, Header.Offsets.ParameterSize,
                (ushort) (2 + paramSize));

            var paramOffset = Header.Template.Length;
            var data = new ByteArray();

            var itemCount = 0;

            foreach (var item in dataItems)
            {
                itemCount++;
                message.Add(Parameter.Template);
                var value = Serialization.SerializeDataItem(item);
                var wordLen = item.Value is bool ? 1 : 2;

                message[paramOffset + Parameter.Offsets.WordLength] = (byte) wordLen;
                Serialization.SetWordAt(message, paramOffset + Parameter.Offsets.Amount, (ushort) value.Length);
                Serialization.SetWordAt(message, paramOffset + Parameter.Offsets.DbNumber, (ushort) item.DB);
                message[paramOffset + Parameter.Offsets.Area] = (byte) item.DataType;

                data.Add(0x00);
                if (item.Value is bool b)
                {
                    if (item.BitAdr > 7)
                        throw new ArgumentException(
                            $"Cannot read bit with invalid {nameof(item.BitAdr)} '{item.BitAdr}'.", nameof(dataItems));

                    Serialization.SetAddressAt(message, paramOffset + Parameter.Offsets.Address, item.StartByteAdr,
                        item.BitAdr);

                    data.Add(0x03);
                    data.AddWord(1);

                    data.Add(b ? (byte)1 : (byte)0);
                    if (itemCount != dataItems.Length) { 
                        data.Add(0);
                    }
                }
                else
                {
                    Serialization.SetAddressAt(message, paramOffset + Parameter.Offsets.Address, item.StartByteAdr, 0);

                    var len = value.Length;
                    data.Add(0x04);
                    data.AddWord((ushort) (len << 3));
                    data.Add(value);
                    
                    if ((len & 0b1) == 1 && itemCount != dataItems.Length)
                    {
                        data.Add(0);
                    }
                }

                paramOffset += Parameter.Template.Length;
            }

            message.Add(data.Array);

            Serialization.SetWordAt(message, Header.Offsets.MessageLength, (ushort) message.Length);
            Serialization.SetWordAt(message, Header.Offsets.DataLength, (ushort) (message.Length - paramOffset));

            return message.Length;
        }

        public static void ParseResponse(byte[] message, int length, DataItem[] dataItems)
        {
            if (length < 12) throw new Exception("Not enough data received to parse write response.");

            var messageError = Serialization.GetWordAt(message, 10);
            if (messageError != 0)
                throw new Exception($"Write failed with error {messageError}.");

            if (length < 14 + dataItems.Length)
                throw new Exception("Not enough data received to parse individual item responses.");

            IList<byte> itemResults = new ArraySegment<byte>(message, 14, dataItems.Length);

            List<Exception>? errors = null;

            for (int i = 0; i < dataItems.Length; i++)
            {
                try
                {
                    Plc.ValidateResponseCode((ReadWriteErrorCode)itemResults[i]);
                }
                catch(Exception e)
                {
                    if (errors == null) errors = new List<Exception>();
                    errors.Add(new Exception($"Write of dataItem {dataItems[i]} failed: {e.Message}."));
                }

            }

            if (errors != null)
                throw new AggregateException(
                    $"Write failed for {errors.Count} items. See the innerExceptions for details.", errors);
        }

        private static class Header
        {
            public static byte[] Template { get; } =
            {
                0x03, 0x00, 0x00, 0x00, // TPKT
                0x02, 0xf0, 0x80, // ISO DT
                0x32, // S7 protocol ID
                0x01, // JobRequest
                0x00, 0x00, // Reserved
                0x05, 0x00, // PDU reference
                0x00, 0x0e, // Parameters length
                0x00, 0x00, // Data length
                0x05, // Function: Write var
                0x00, // Number of items to write
            };

            public static class Offsets
            {
                public const int MessageLength = 2;
                public const int ParameterSize = 13;
                public const int DataLength = 15;
                public const int ParameterCount = 18;
            }
        }

        private static class Parameter
        {
            public static byte[] Template { get; } =
            {
                0x12, // Spec
                0x0a, // Length of remaining bytes
                0x10, // Addressing mode
                0x02, // Transport size
                0x00, 0x00, // Number of elements
                0x00, 0x00, // DB number
                0x84, // Area type
                0x00, 0x00, 0x00 // Area offset
            };

            public static class Offsets
            {
                public const int WordLength = 3;
                public const int Amount = 4;
                public const int DbNumber = 6;
                public const int Area = 8;
                public const int Address = 9;
            }
        }
    }
}
