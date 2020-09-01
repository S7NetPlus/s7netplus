using S7.Net.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using S7.Net.Protocol;
using System.IO;

namespace S7.Net
{
    /// <summary>
    /// Creates an instance of S7.Net driver
    /// </summary>
    public partial class Plc
    {
        /// <summary>
        /// Connects to the PLC and performs a COTP ConnectionRequest and S7 CommunicationSetup.
        /// </summary>
        /// <returns>A task that represents the asynchronous open operation.</returns>
        public async Task OpenAsync()
        {
            await ConnectAsync();
            var stream = GetStreamIfAvailable();

            await stream.WriteAsync(ConnectionRequest.GetCOTPConnectionRequest(CPU, Rack, Slot), 0, 22);
            var response = await COTP.TPDU.ReadAsync(stream);
            if (response == null)
            {
                throw new Exception("Error reading Connection Confirm. Malformed TPDU packet");
            }
            if (response.PDUType != 0xd0) //Connect Confirm
            {
                throw new InvalidDataException("Error reading Connection Confirm", response.TPkt.Data, 1, 0x0d);
            }

            await stream.WriteAsync(GetS7ConnectionSetup(), 0, 25);

            var s7data = await COTP.TSDU.ReadAsync(stream);
            if (s7data == null)
                throw new WrongNumberOfBytesException("No data received in response to Communication Setup");

            //Check for S7 Ack Data
            if (s7data[1] != 0x03)
                throw new InvalidDataException("Error reading Communication Setup response", s7data, 1, 0x03);

            MaxPDUSize = (short)(s7data[18] * 256 + s7data[19]);
        }

        private async Task ConnectAsync()
        {
            tcpClient = new TcpClient();
            ConfigureConnection();
            await tcpClient.ConnectAsync(IP, Port);
            _stream = tcpClient.GetStream();
        }


        /// <summary>
        /// Reads a number of bytes from a DB starting from a specified index. This handles more than 200 bytes with multiple requests.
        /// If the read was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <param name="count">Byte count, if you want to read 120 bytes, set this to 120.</param>
        /// <returns>Returns the bytes in an array</returns>
        public async Task<byte[]> ReadBytesAsync(DataType dataType, int db, int startByteAdr, int count)
        {
            List<byte> resultBytes = new List<byte>();
            int index = startByteAdr;
            while (count > 0)
            {
                //This works up to MaxPDUSize-1 on SNAP7. But not MaxPDUSize-0.
                var maxToRead = (int)Math.Min(count, MaxPDUSize - 18);
                byte[] bytes = await ReadBytesWithSingleRequestAsync(dataType, db, index, maxToRead);
                if (bytes == null)
                    return resultBytes.ToArray();
                resultBytes.AddRange(bytes);
                count -= maxToRead;
                index += maxToRead;
            }
            return resultBytes.ToArray();
        }

        /// <summary>
        /// Read and decode a certain number of bytes of the "VarType" provided. 
        /// This can be used to read multiple consecutive variables of the same type (Word, DWord, Int, etc).
        /// If the read was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <param name="varType">Type of the variable/s that you are reading</param>
        /// <param name="bitAdr">Address of bit. If you want to read DB1.DBX200.6, set 6 to this parameter.</param>
        /// <param name="varCount"></param>
        public async Task<object?> ReadAsync(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0)
        {
            int cntBytes = VarTypeToByteLength(varType, varCount);
            byte[] bytes = await ReadBytesAsync(dataType, db, startByteAdr, cntBytes);
            return ParseBytes(varType, bytes, varCount, bitAdr);
        }

        /// <summary>
        /// Reads a single variable from the PLC, takes in input strings like "DB1.DBX0.0", "DB20.DBD200", "MB20", "T45", etc.
        /// If the read was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="variable">Input strings like "DB1.DBX0.0", "DB20.DBD200", "MB20", "T45", etc.</param>
        /// <returns>Returns an object that contains the value. This object must be cast accordingly.</returns>
        public async Task<object?> ReadAsync(string variable)
        {
            var adr = new PLCAddress(variable);
            return await ReadAsync(adr.DataType, adr.DbNumber, adr.StartByte, adr.VarType, 1, (byte)adr.BitNumber);
        }

        /// <summary>
        /// Reads all the bytes needed to fill a struct in C#, starting from a certain address, and return an object that can be casted to the struct.
        /// </summary>
        /// <param name="structType">Type of the struct to be readed (es.: TypeOf(MyStruct)).</param>
        /// <param name="db">Address of the DB.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <returns>Returns a struct that must be cast.</returns>
        public async Task<object?> ReadStructAsync(Type structType, int db, int startByteAdr = 0)
        {
            int numBytes = Types.Struct.GetStructSize(structType);
            // now read the package
            var resultBytes = await ReadBytesAsync(DataType.DataBlock, db, startByteAdr, numBytes);

            // and decode it
            return Types.Struct.FromBytes(structType, resultBytes);
        }

        /// <summary>
        /// Reads all the bytes needed to fill a struct in C#, starting from a certain address, and returns the struct or null if nothing was read.
        /// </summary>
        /// <typeparam name="T">The struct type</typeparam>
        /// <param name="db">Address of the DB.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <returns>Returns a nulable struct. If nothing was read null will be returned.</returns>
        public async Task<T?> ReadStructAsync<T>(int db, int startByteAdr = 0) where T : struct
        {
            return await ReadStructAsync(typeof(T), db, startByteAdr) as T?;
        }

        /// <summary>
        /// Reads all the bytes needed to fill a class in C#, starting from a certain address, and set all the properties values to the value that are read from the PLC. 
        /// This reads only properties, it doesn't read private variable or public variable without {get;set;} specified.
        /// </summary>
        /// <param name="sourceClass">Instance of the class that will store the values</param>       
        /// <param name="db">Index of the DB; es.: 1 is for DB1</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <returns>The number of read bytes</returns>
        public async Task<Tuple<int, object>> ReadClassAsync(object sourceClass, int db, int startByteAdr = 0)
        {
            int numBytes = (int)Class.GetClassSize(sourceClass);
            if (numBytes <= 0)
            {
                throw new Exception("The size of the class is less than 1 byte and therefore cannot be read");
            }

            // now read the package
            var resultBytes = await ReadBytesAsync(DataType.DataBlock, db, startByteAdr, numBytes);
            // and decode it
            Class.FromBytes(sourceClass, resultBytes);

            return new Tuple<int, object>(resultBytes.Length, sourceClass);
        }

        /// <summary>
        /// Reads all the bytes needed to fill a class in C#, starting from a certain address, and set all the properties values to the value that are read from the PLC. 
        /// This reads only properties, it doesn't read private variable or public variable without {get;set;} specified. To instantiate the class defined by the generic
        /// type, the class needs a default constructor.
        /// </summary>
        /// <typeparam name="T">The class that will be instantiated. Requires a default constructor</typeparam>
        /// <param name="db">Index of the DB; es.: 1 is for DB1</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <returns>An instance of the class with the values read from the PLC. If no data has been read, null will be returned</returns>
        public async Task<T?> ReadClassAsync<T>(int db, int startByteAdr = 0) where T : class
        {
            return await ReadClassAsync(() => Activator.CreateInstance<T>(), db, startByteAdr);
        }

        /// <summary>
        /// Reads all the bytes needed to fill a class in C#, starting from a certain address, and set all the properties values to the value that are read from the PLC. 
        /// This reads only properties, it doesn't read private variable or public variable without {get;set;} specified.
        /// </summary>
        /// <typeparam name="T">The class that will be instantiated</typeparam>
        /// <param name="classFactory">Function to instantiate the class</param>
        /// <param name="db">Index of the DB; es.: 1 is for DB1</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <returns>An instance of the class with the values read from the PLC. If no data has been read, null will be returned</returns>
        public async Task<T?> ReadClassAsync<T>(Func<T> classFactory, int db, int startByteAdr = 0) where T : class
        {
            var instance = classFactory();
            var res = await ReadClassAsync(instance, db, startByteAdr);
            int readBytes = res.Item1;
            if (readBytes <= 0)
            {
                return null;
            }

            return (T)res.Item2;
        }

        /// <summary>
        /// Reads multiple vars in a single request. 
        /// You have to create and pass a list of DataItems and you obtain in response the same list with the values.
        /// Values are stored in the property "Value" of the dataItem and are already converted.
        /// If you don't want the conversion, just create a dataItem of bytes. 
        /// DataItems must not be more than 20 (protocol restriction) and bytes must not be more than 200 + 22 of header (protocol restriction).
        /// </summary>
        /// <param name="dataItems">List of dataitems that contains the list of variables that must be read. Maximum 20 dataitems are accepted.</param>
        public async Task<List<DataItem>> ReadMultipleVarsAsync(List<DataItem> dataItems)
        {
            //Snap7 seems to choke on PDU sizes above 256 even if snap7 
            //replies with bigger PDU size in connection setup.
            AssertPduSizeForRead(dataItems);

            var stream = GetStreamIfAvailable();

            try
            {
                // first create the header
                int packageSize = 19 + (dataItems.Count * 12);
                ByteArray package = new ByteArray(packageSize);
                package.Add(ReadHeaderPackage(dataItems.Count));
                // package.Add(0x02);  // datenart
                foreach (var dataItem in dataItems)
                {
                    package.Add(CreateReadDataRequestPackage(dataItem.DataType, dataItem.DB, dataItem.StartByteAdr, VarTypeToByteLength(dataItem.VarType, dataItem.Count)));
                }

                await stream.WriteAsync(package.Array, 0, package.Array.Length);

                var s7data = await COTP.TSDU.ReadAsync(stream); //TODO use Async
                if (s7data == null || s7data[14] != 0xff)
                    throw new PlcException(ErrorCode.WrongNumberReceivedBytes);

                ParseDataIntoDataItems(s7data, dataItems);
            }
            catch (SocketException socketException)
            {
                throw new PlcException(ErrorCode.ReadData, socketException);
            }
            catch (Exception exc)
            {
                throw new PlcException(ErrorCode.ReadData, exc);
            }
            return dataItems;
        }

        /// <summary>
        /// Write a number of bytes from a DB starting from a specified index. This handles more than 200 bytes with multiple requests.
        /// If the write was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to write DB1.DBW200, this is 200.</param>
        /// <param name="value">Bytes to write. If more than 200, multiple requests will be made.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public async Task WriteBytesAsync(DataType dataType, int db, int startByteAdr, byte[] value)
        {
            int localIndex = 0;
            int count = value.Length;
            while (count > 0)
            {
                //TODO: Figure out how to use MaxPDUSize here
                //Snap7 seems to choke on PDU sizes above 256 even if snap7 
                //replies with bigger PDU size in connection setup.
                var maxToWrite = (int)Math.Min(count, 200);
                await WriteBytesWithASingleRequestAsync(dataType, db, startByteAdr + localIndex, value.Skip(localIndex).Take(maxToWrite).ToArray());
                count -= maxToWrite;
                localIndex += maxToWrite;
            }
        }

        /// <summary>
        /// Write a single bit from a DB with the specified index.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to write DB1.DBW200, this is 200.</param>
        /// <param name="bitAdr">The address of the bit. (0-7)</param>
        /// <param name="value">Bytes to write. If more than 200, multiple requests will be made.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public async Task WriteBitAsync(DataType dataType, int db, int startByteAdr, int bitAdr, bool value)
        {
            if (bitAdr < 0 || bitAdr > 7)
                throw new InvalidAddressException(string.Format("Addressing Error: You can only reference bitwise locations 0-7. Address {0} is invalid", bitAdr));

            await WriteBitWithASingleRequestAsync(dataType, db, startByteAdr, bitAdr, value);
        }

        /// <summary>
        /// Write a single bit from a DB with the specified index.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to write DB1.DBW200, this is 200.</param>
        /// <param name="bitAdr">The address of the bit. (0-7)</param>
        /// <param name="value">Bytes to write. If more than 200, multiple requests will be made.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public async Task WriteBitAsync(DataType dataType, int db, int startByteAdr, int bitAdr, int value)
        {
            if (value < 0 || value > 1)
                throw new ArgumentException("Value must be 0 or 1", nameof(value));

            await WriteBitAsync(dataType, db, startByteAdr, bitAdr, value == 1);
        }

        /// <summary>
        /// Takes in input an object and tries to parse it to an array of values. This can be used to write many data, all of the same type.
        /// You must specify the memory area type, memory are address, byte start address and bytes count.
        /// If the read was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <param name="value">Bytes to write. The lenght of this parameter can't be higher than 200. If you need more, use recursion.</param>
        /// <param name="bitAdr">The address of the bit. (0-7)</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public async Task WriteAsync(DataType dataType, int db, int startByteAdr, object value, int bitAdr = -1)
        {
            if (bitAdr != -1)
            {
                //Must be writing a bit value as bitAdr is specified
                if (value is bool)
                {
                    await WriteBitAsync(dataType, db, startByteAdr, bitAdr, (bool) value);
                }
                else if (value is int intValue)
                {
                    if (intValue < 0 || intValue > 7)
                        throw new ArgumentOutOfRangeException(
                            string.Format(
                                "Addressing Error: You can only reference bitwise locations 0-7. Address {0} is invalid",
                                bitAdr), nameof(bitAdr));

                    await WriteBitAsync(dataType, db, startByteAdr, bitAdr, intValue == 1);
                }
                else throw new ArgumentException("Value must be a bool or an int to write a bit", nameof(value));
            }
            else await WriteBytesAsync(dataType, db, startByteAdr, Serialization.SerializeValue(value));
        }

        /// <summary>
        /// Writes a single variable from the PLC, takes in input strings like "DB1.DBX0.0", "DB20.DBD200", "MB20", "T45", etc.
        /// If the write was not successful, check <see cref="LastErrorCode"/> or <see cref="LastErrorString"/>.
        /// </summary>
        /// <param name="variable">Input strings like "DB1.DBX0.0", "DB20.DBD200", "MB20", "T45", etc.</param>
        /// <param name="value">Value to be written to the PLC</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public async Task WriteAsync(string variable, object value)
        {
            var adr = new PLCAddress(variable);
            await WriteAsync(adr.DataType, adr.DbNumber, adr.StartByte, value, adr.BitNumber);
        }

        /// <summary>
        /// Writes a C# struct to a DB in the PLC
        /// </summary>
        /// <param name="structValue">The struct to be written</param>
        /// <param name="db">Db address</param>
        /// <param name="startByteAdr">Start bytes on the PLC</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public async Task WriteStructAsync(object structValue, int db, int startByteAdr = 0)
        {
            var bytes = Struct.ToBytes(structValue).ToList();
            await WriteBytesAsync(DataType.DataBlock, db, startByteAdr, bytes.ToArray());
        }

        /// <summary>
        /// Writes a C# class to a DB in the PLC
        /// </summary>
        /// <param name="classValue">The class to be written</param>
        /// <param name="db">Db address</param>
        /// <param name="startByteAdr">Start bytes on the PLC</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public async Task WriteClassAsync(object classValue, int db, int startByteAdr = 0)
        {
            byte[] bytes = new byte[(int)Class.GetClassSize(classValue)];
            Types.Class.ToBytes(classValue, bytes);
            await WriteBytesAsync(DataType.DataBlock, db, startByteAdr, bytes);
        }

        private async Task<byte[]> ReadBytesWithSingleRequestAsync(DataType dataType, int db, int startByteAdr, int count)
        {
            var stream = GetStreamIfAvailable();

            byte[] bytes = new byte[count];

            // first create the header
            int packageSize = 31;
            ByteArray package = new ByteArray(packageSize);
            package.Add(ReadHeaderPackage());
            // package.Add(0x02);  // datenart
            package.Add(CreateReadDataRequestPackage(dataType, db, startByteAdr, count));

            await stream.WriteAsync(package.Array, 0, package.Array.Length);

            var s7data = await COTP.TSDU.ReadAsync(stream);
            AssertReadResponse(s7data, count);

            for (int cnt = 0; cnt < count; cnt++)
                bytes[cnt] = s7data[cnt + 18];

            return bytes;
        }

        /// <summary>
        /// Write DataItem(s) to the PLC. Throws an exception if the response is invalid
        /// or when the PLC reports errors for item(s) written.
        /// </summary>
        /// <param name="dataItems">The DataItem(s) to write to the PLC.</param>
        /// <returns>Task that completes when response from PLC is parsed.</returns>
        public async Task WriteAsync(params DataItem[] dataItems)
        {
            AssertPduSizeForWrite(dataItems);

            var stream = GetStreamIfAvailable();

            var message = new ByteArray();
            var length = S7WriteMultiple.CreateRequest(message, dataItems);
            await stream.WriteAsync(message.Array, 0, length).ConfigureAwait(false);

            var response = await COTP.TSDU.ReadAsync(stream).ConfigureAwait(false);
            S7WriteMultiple.ParseResponse(response, response.Length, dataItems);
        }

        /// <summary>
        /// Writes up to 200 bytes to the PLC. You must specify the memory area type, memory are address, byte start address and bytes count.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <param name="value">Bytes to write. The lenght of this parameter can't be higher than 200. If you need more, use recursion.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        private async Task WriteBytesWithASingleRequestAsync(DataType dataType, int db, int startByteAdr, byte[] value)
        {
            var stream = GetStreamIfAvailable();

            byte[] bReceive = new byte[513];
            int varCount = 0;

            try
            {
                varCount = value.Length;
                // first create the header
                int packageSize = 35 + value.Length;
                ByteArray package = new ByteArray(packageSize);

                package.Add(new byte[] { 3, 0, 0 });
                package.Add((byte)packageSize);
                package.Add(new byte[] { 2, 0xf0, 0x80, 0x32, 1, 0, 0 });
                package.Add(Word.ToByteArray((ushort)(varCount - 1)));
                package.Add(new byte[] { 0, 0x0e });
                package.Add(Word.ToByteArray((ushort)(varCount + 4)));
                package.Add(new byte[] { 0x05, 0x01, 0x12, 0x0a, 0x10, 0x02 });
                package.Add(Word.ToByteArray((ushort)varCount));
                package.Add(Word.ToByteArray((ushort)(db)));
                package.Add((byte)dataType);
                var overflow = (int)(startByteAdr * 8 / 0xffffU); // handles words with address bigger than 8191
                package.Add((byte)overflow);
                package.Add(Word.ToByteArray((ushort)(startByteAdr * 8)));
                package.Add(new byte[] { 0, 4 });
                package.Add(Word.ToByteArray((ushort)(varCount * 8)));

                // now join the header and the data
                package.Add(value);

                await stream.WriteAsync(package.Array, 0, package.Array.Length);

                var s7data = await COTP.TSDU.ReadAsync(stream);
                if (s7data == null || s7data[14] != 0xff)
                {
                    throw new PlcException(ErrorCode.WrongNumberReceivedBytes);
                }
            }
            catch (Exception exc)
            {
                throw new PlcException(ErrorCode.WriteData, exc);
            }
        }

        private async Task WriteBitWithASingleRequestAsync(DataType dataType, int db, int startByteAdr, int bitAdr, bool bitValue)
        {
            var stream = GetStreamIfAvailable();

            byte[] bReceive = new byte[513];
            int varCount = 0;

            try
            {
                var value = new[] {bitValue ? (byte) 1 : (byte) 0};
                varCount = value.Length;
                // first create the header
                int packageSize = 35 + value.Length;
                ByteArray package = new Types.ByteArray(packageSize);

                package.Add(new byte[] { 3, 0, 0 });
                package.Add((byte)packageSize);
                package.Add(new byte[] { 2, 0xf0, 0x80, 0x32, 1, 0, 0 });
                package.Add(Word.ToByteArray((ushort)(varCount - 1)));
                package.Add(new byte[] { 0, 0x0e });
                package.Add(Word.ToByteArray((ushort)(varCount + 4)));
                package.Add(new byte[] { 0x05, 0x01, 0x12, 0x0a, 0x10, 0x01 }); //ending 0x01 is used for writing a sinlge bit
                package.Add(Word.ToByteArray((ushort)varCount));
                package.Add(Word.ToByteArray((ushort)(db)));
                package.Add((byte)dataType);
                int overflow = (int)(startByteAdr * 8 / 0xffffU); // handles words with address bigger than 8191
                package.Add((byte)overflow);
                package.Add(Word.ToByteArray((ushort)(startByteAdr * 8 + bitAdr)));
                package.Add(new byte[] { 0, 0x03 }); //ending 0x03 is used for writing a sinlge bit
                package.Add(Word.ToByteArray((ushort)(varCount)));

                // now join the header and the data
                package.Add(value);

                await stream.WriteAsync(package.Array, 0, package.Array.Length);

                var s7data = await COTP.TSDU.ReadAsync(stream);
                if (s7data == null || s7data[14] != 0xff)
                    throw new PlcException(ErrorCode.WrongNumberReceivedBytes);
            }
            catch (Exception exc)
            {
                throw new PlcException(ErrorCode.WriteData, exc);
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
    }
}
