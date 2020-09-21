using S7.Net.Types;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using S7.Net.Protocol;
using S7.Net.Helper;

//Implement synchronous methods here
namespace S7.Net
{
    public partial class Plc
    {
        /// <summary>
        /// Connects to the PLC and performs a COTP ConnectionRequest and S7 CommunicationSetup.
        /// </summary>
        public void Open()
        {
            try
            {
                OpenAsync().GetAwaiter().GetResult();
            }
            catch (Exception exc)
            {
                throw new PlcException(ErrorCode.ConnectionError,
                    $"Couldn't establish the connection to {IP}.\nMessage: {exc.Message}", exc);
            }
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
        public byte[] ReadBytes(DataType dataType, int db, int startByteAdr, int count)
        {
            var result = new byte[count];
            int index = 0;
            while (count > 0)
            {
                //This works up to MaxPDUSize-1 on SNAP7. But not MaxPDUSize-0.
                var maxToRead = Math.Min(count, MaxPDUSize - 18);
                ReadBytesWithSingleRequest(dataType, db, startByteAdr + index, result, index, maxToRead);
                count -= maxToRead;
                index += maxToRead;
            }
            return result;
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
        public object? Read(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0)
        {
            int cntBytes = VarTypeToByteLength(varType, varCount);
            byte[] bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);

            return ParseBytes(varType, bytes, varCount, bitAdr);
        }

        /// <summary>
        /// Reads a single variable from the PLC, takes in input strings like "DB1.DBX0.0", "DB20.DBD200", "MB20", "T45", etc.
        /// If the read was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="variable">Input strings like "DB1.DBX0.0", "DB20.DBD200", "MB20", "T45", etc.</param>
        /// <returns>Returns an object that contains the value. This object must be cast accordingly. If no data has been read, null will be returned</returns>
        public object? Read(string variable)
        {
            var adr = new PLCAddress(variable);
            return Read(adr.DataType, adr.DbNumber, adr.StartByte, adr.VarType, 1, (byte)adr.BitNumber);
        }

        /// <summary>
        /// Reads all the bytes needed to fill a struct in C#, starting from a certain address, and return an object that can be casted to the struct.
        /// </summary>
        /// <param name="structType">Type of the struct to be readed (es.: TypeOf(MyStruct)).</param>
        /// <param name="db">Address of the DB.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <returns>Returns a struct that must be cast. If no data has been read, null will be returned</returns>
        public object? ReadStruct(Type structType, int db, int startByteAdr = 0)
        {
            int numBytes = Struct.GetStructSize(structType);
            // now read the package
            var resultBytes = ReadBytes(DataType.DataBlock, db, startByteAdr, numBytes);

            // and decode it
            return Struct.FromBytes(structType, resultBytes);
        }

        /// <summary>
        /// Reads all the bytes needed to fill a struct in C#, starting from a certain address, and returns the struct or null if nothing was read.
        /// </summary>
        /// <typeparam name="T">The struct type</typeparam>
        /// <param name="db">Address of the DB.</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <returns>Returns a nullable struct. If nothing was read null will be returned.</returns>
        public T? ReadStruct<T>(int db, int startByteAdr = 0) where T : struct
        {
            return ReadStruct(typeof(T), db, startByteAdr) as T?;
        }


        /// <summary>
        /// Reads all the bytes needed to fill a class in C#, starting from a certain address, and set all the properties values to the value that are read from the PLC. 
        /// This reads only properties, it doesn't read private variable or public variable without {get;set;} specified.
        /// </summary>
        /// <param name="sourceClass">Instance of the class that will store the values</param>       
        /// <param name="db">Index of the DB; es.: 1 is for DB1</param>
        /// <param name="startByteAdr">Start byte address. If you want to read DB1.DBW200, this is 200.</param>
        /// <returns>The number of read bytes</returns>
        public int ReadClass(object sourceClass, int db, int startByteAdr = 0)
        {
            int numBytes = (int)Class.GetClassSize(sourceClass);
            if (numBytes <= 0)
            {
                throw new Exception("The size of the class is less than 1 byte and therefore cannot be read");
            }

            // now read the package
            var resultBytes = ReadBytes(DataType.DataBlock, db, startByteAdr, numBytes);
            // and decode it
            Class.FromBytes(sourceClass, resultBytes);
            return resultBytes.Length;
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
        public T? ReadClass<T>(int db, int startByteAdr = 0) where T : class
        {
            return ReadClass(() => Activator.CreateInstance<T>(), db, startByteAdr);
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
        public T? ReadClass<T>(Func<T> classFactory, int db, int startByteAdr = 0) where T : class
        {
            var instance = classFactory();
            int readBytes = ReadClass(instance, db, startByteAdr);
            if (readBytes <= 0)
            {
                return null;
            }
            return instance;
        }

        /// <summary>
        /// Write a number of bytes from a DB starting from a specified index. This handles more than 200 bytes with multiple requests.
        /// If the write was not successful, check LastErrorCode or LastErrorString.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to read DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to write DB1.DBW200, this is 200.</param>
        /// <param name="value">Bytes to write. If more than 200, multiple requests will be made.</param>
        public void WriteBytes(DataType dataType, int db, int startByteAdr, byte[] value)
        {
            int localIndex = 0;
            int count = value.Length;
            while (count > 0)
            {
                //TODO: Figure out how to use MaxPDUSize here
                //Snap7 seems to choke on PDU sizes above 256 even if snap7 
                //replies with bigger PDU size in connection setup.
                var maxToWrite = Math.Min(count, MaxPDUSize - 28);//TODO tested only when the MaxPDUSize is 480
                WriteBytesWithASingleRequest(dataType, db, startByteAdr + localIndex, value, localIndex, maxToWrite);
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
        public void WriteBit(DataType dataType, int db, int startByteAdr, int bitAdr, bool value)
        {
            if (bitAdr < 0 || bitAdr > 7)
                throw new InvalidAddressException(string.Format("Addressing Error: You can only reference bitwise locations 0-7. Address {0} is invalid", bitAdr));

            WriteBitWithASingleRequest(dataType, db, startByteAdr, bitAdr, value);
        }

        /// <summary>
        /// Write a single bit to a DB with the specified index.
        /// </summary>
        /// <param name="dataType">Data type of the memory area, can be DB, Timer, Counter, Merker(Memory), Input, Output.</param>
        /// <param name="db">Address of the memory area (if you want to write DB1, this is set to 1). This must be set also for other memory area types: counters, timers,etc.</param>
        /// <param name="startByteAdr">Start byte address. If you want to write DB1.DBW200, this is 200.</param>
        /// <param name="bitAdr">The address of the bit. (0-7)</param>
        /// <param name="value">Value to write (0 or 1).</param>
        public void WriteBit(DataType dataType, int db, int startByteAdr, int bitAdr, int value)
        {
            if (value < 0 || value > 1)
                throw new ArgumentException("Value must be 0 or 1", nameof(value));

            WriteBit(dataType, db, startByteAdr, bitAdr, value == 1);
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
        public void Write(DataType dataType, int db, int startByteAdr, object value, int bitAdr = -1)
        {
            if (bitAdr != -1)
            {
                //Must be writing a bit value as bitAdr is specified
                if (value is bool boolean)
                {
                    WriteBit(dataType, db, startByteAdr, bitAdr, boolean);
                }
                else if (value is int intValue)
                {
                    if (intValue < 0 || intValue > 7)
                        throw new ArgumentOutOfRangeException(
                            string.Format(
                                "Addressing Error: You can only reference bitwise locations 0-7. Address {0} is invalid",
                                bitAdr), nameof(bitAdr));

                    WriteBit(dataType, db, startByteAdr, bitAdr, intValue == 1);
                }
                else
                    throw new ArgumentException("Value must be a bool or an int to write a bit", nameof(value));
            }
            else WriteBytes(dataType, db, startByteAdr, Serialization.SerializeValue(value));
        }

        /// <summary>
        /// Writes a single variable from the PLC, takes in input strings like "DB1.DBX0.0", "DB20.DBD200", "MB20", "T45", etc.
        /// If the write was not successful, check <see cref="LastErrorCode"/> or <see cref="LastErrorString"/>.
        /// </summary>
        /// <param name="variable">Input strings like "DB1.DBX0.0", "DB20.DBD200", "MB20", "T45", etc.</param>
        /// <param name="value">Value to be written to the PLC</param>
        public void Write(string variable, object value)
        {
            var adr = new PLCAddress(variable);
            Write(adr.DataType, adr.DbNumber, adr.StartByte, value, adr.BitNumber);
        }

        /// <summary>
        /// Writes a C# struct to a DB in the PLC
        /// </summary>
        /// <param name="structValue">The struct to be written</param>
        /// <param name="db">Db address</param>
        /// <param name="startByteAdr">Start bytes on the PLC</param>
        public void WriteStruct(object structValue, int db, int startByteAdr = 0)
        {
            WriteStructAsync(structValue, db, startByteAdr).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Writes a C# class to a DB in the PLC
        /// </summary>
        /// <param name="classValue">The class to be written</param>
        /// <param name="db">Db address</param>
        /// <param name="startByteAdr">Start bytes on the PLC</param>
        public void WriteClass(object classValue, int db, int startByteAdr = 0)
        {
            WriteClassAsync(classValue, db, startByteAdr).GetAwaiter().GetResult();
        }

        private void ReadBytesWithSingleRequest(DataType dataType, int db, int startByteAdr, byte[] buffer, int offset, int count)
        {
            var stream = GetStreamIfAvailable();
            try
            {
                // first create the header
                int packageSize = 19 + 12; // 19 header + 12 for 1 request
                var package = new System.IO.MemoryStream(packageSize);
                BuildHeaderPackage(package);
                // package.Add(0x02);  // datenart
                BuildReadDataRequestPackage(package, dataType, db, startByteAdr, count);

                var dataToSend = package.ToArray();
                stream.Write(dataToSend, 0, dataToSend.Length);

                var s7data = COTP.TSDU.Read(stream);
                AssertReadResponse(s7data, count);

                Array.Copy(s7data, 18, buffer, offset, count);
            }
            catch (Exception exc)
            {
                throw new PlcException(ErrorCode.ReadData, exc);
            }
        }

        /// <summary>
        /// Write DataItem(s) to the PLC. Throws an exception if the response is invalid
        /// or when the PLC reports errors for item(s) written.
        /// </summary>
        /// <param name="dataItems">The DataItem(s) to write to the PLC.</param>
        public void Write(params DataItem[] dataItems)
        {
            AssertPduSizeForWrite(dataItems);

            var stream = GetStreamIfAvailable();

            var message = new ByteArray();
            var length = S7WriteMultiple.CreateRequest(message, dataItems);
            stream.Write(message.Array, 0, length);

            var response = COTP.TSDU.Read(stream);
            S7WriteMultiple.ParseResponse(response, response.Length, dataItems);
        }

        private void WriteBytesWithASingleRequest(DataType dataType, int db, int startByteAdr, byte[] value, int dataOffset, int count)
        {
            try
            {
                var stream = GetStreamIfAvailable();
                var dataToSend = BuildWriteBytesPackage(dataType, db, startByteAdr, value, dataOffset, count);

                stream.Write(dataToSend, 0, dataToSend.Length);

                var s7data = COTP.TSDU.Read(stream);
                ValidateResponseCode((ReadWriteErrorCode)s7data[14]);
            }
            catch (Exception exc)
            {
                throw new PlcException(ErrorCode.WriteData, exc);
            }
        }

        private byte[] BuildWriteBytesPackage(DataType dataType, int db, int startByteAdr, byte[] value, int dataOffset, int count)
        {
            int varCount = count;
            // first create the header
            int packageSize = 35 + varCount;
            var package = new MemoryStream(new byte[packageSize]);

            package.WriteByte(3);
            package.WriteByte(0);
            //complete package size
            package.WriteByteArray(Int.ToByteArray((short)packageSize));
            package.WriteByteArray(new byte[] { 2, 0xf0, 0x80, 0x32, 1, 0, 0 });
            package.WriteByteArray(Word.ToByteArray((ushort)(varCount - 1)));
            package.WriteByteArray(new byte[] { 0, 0x0e });
            package.WriteByteArray(Word.ToByteArray((ushort)(varCount + 4)));
            package.WriteByteArray(new byte[] { 0x05, 0x01, 0x12, 0x0a, 0x10, 0x02 });
            package.WriteByteArray(Word.ToByteArray((ushort)varCount));
            package.WriteByteArray(Word.ToByteArray((ushort)(db)));
            package.WriteByte((byte)dataType);
            var overflow = (int)(startByteAdr * 8 / 0xffffU); // handles words with address bigger than 8191
            package.WriteByte((byte)overflow);
            package.WriteByteArray(Word.ToByteArray((ushort)(startByteAdr * 8)));
            package.WriteByteArray(new byte[] { 0, 4 });
            package.WriteByteArray(Word.ToByteArray((ushort)(varCount * 8)));

            // now join the header and the data
            package.Write(value, dataOffset, count);

            return package.ToArray();
        }

        private byte[] BuildWriteBitPackage(DataType dataType, int db, int startByteAdr, bool bitValue, int bitAdr)
        {
            var value = new[] { bitValue ? (byte)1 : (byte)0 };
            int varCount = 1;
            // first create the header
            int packageSize = 35 + varCount;
            var package = new MemoryStream(new byte[packageSize]);

            package.WriteByte(3);
            package.WriteByte(0);
            //complete package size
            package.WriteByteArray(Int.ToByteArray((short)packageSize));
            package.WriteByteArray(new byte[] { 2, 0xf0, 0x80, 0x32, 1, 0, 0 });
            package.WriteByteArray(Word.ToByteArray((ushort)(varCount - 1)));
            package.WriteByteArray(new byte[] { 0, 0x0e });
            package.WriteByteArray(Word.ToByteArray((ushort)(varCount + 4)));
            package.WriteByteArray(new byte[] { 0x05, 0x01, 0x12, 0x0a, 0x10, 0x01 }); //ending 0x01 is used for writing a sinlge bit
            package.WriteByteArray(Word.ToByteArray((ushort)varCount));
            package.WriteByteArray(Word.ToByteArray((ushort)(db)));
            package.WriteByte((byte)dataType);
            var overflow = (int)(startByteAdr * 8 / 0xffffU); // handles words with address bigger than 8191
            package.WriteByte((byte)overflow);
            package.WriteByteArray(Word.ToByteArray((ushort)(startByteAdr * 8 + bitAdr)));
            package.WriteByteArray(new byte[] { 0, 0x03 }); //ending 0x03 is used for writing a sinlge bit
            package.WriteByteArray(Word.ToByteArray((ushort)(varCount)));

            // now join the header and the data
            package.WriteByteArray(value);

            return package.ToArray();
        }


        private void WriteBitWithASingleRequest(DataType dataType, int db, int startByteAdr, int bitAdr, bool bitValue)
        {
            var stream = GetStreamIfAvailable();
            try
            {
                var dataToSend = BuildWriteBitPackage(dataType, db, startByteAdr, bitValue, bitAdr);

                stream.Write(dataToSend, 0, dataToSend.Length);

                var s7data = COTP.TSDU.Read(stream);
                ValidateResponseCode((ReadWriteErrorCode)s7data[14]);
            }
            catch (Exception exc)
            {
                throw new PlcException(ErrorCode.WriteData, exc);
            }
        }

        /// <summary>
        /// Reads multiple vars in a single request. 
        /// You have to create and pass a list of DataItems and you obtain in response the same list with the values.
        /// Values are stored in the property "Value" of the dataItem and are already converted.
        /// If you don't want the conversion, just create a dataItem of bytes. 
        /// The number of DataItems as well as the total size of the requested data can not exceed a certain limit (protocol restriction).
        /// </summary>
        /// <param name="dataItems">List of dataitems that contains the list of variables that must be read.</param>
        public void ReadMultipleVars(List<DataItem> dataItems)
        {
            AssertPduSizeForRead(dataItems);

            var stream = GetStreamIfAvailable();

            try
            {
                // first create the header
                int packageSize = 19 + (dataItems.Count * 12);
                var package = new System.IO.MemoryStream(packageSize);
                BuildHeaderPackage(package, dataItems.Count);
                // package.Add(0x02);  // datenart
                foreach (var dataItem in dataItems)
                {
                    BuildReadDataRequestPackage(package, dataItem.DataType, dataItem.DB, dataItem.StartByteAdr, VarTypeToByteLength(dataItem.VarType, dataItem.Count));
                }

                var dataToSend = package.ToArray();
                stream.Write(dataToSend, 0, dataToSend.Length);

                var s7data = COTP.TSDU.Read(stream); //TODO use Async

                ValidateResponseCode((ReadWriteErrorCode)s7data[14]);

                ParseDataIntoDataItems(s7data, dataItems);
            }
            catch (Exception exc)
            {
                throw new PlcException(ErrorCode.ReadData, exc);
            }
        }
    }
}
