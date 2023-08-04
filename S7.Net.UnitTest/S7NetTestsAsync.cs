#region Using
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net.UnitTest.Helpers;
using S7.Net.Types;
using S7.UnitTest.Helpers;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;


#if NET5_0_OR_GREATER
using System.Buffers;
#endif

#endregion

/**
 * About the tests:
 * ---------------------------------------------------------------------------
 * The tests were written to show how to use this library to read and write
 * different types of values, how to box and unbox values and of course to
 * address some of the bugs of the library.
 * These tests are not meant to cover 100% the code, but to check that once a
 * variable is written, it stores the correct value.
 * ----------------------------------------------------------------------------
 * The plc used for the tests is the S7 "server" provided by Snap7 opensource
 * library, that you can get for free here:http://snap7.sourceforge.net/
 * The implementation of the server will not be discussed here, but there are
 * some issues with the interop that cause the server, and unit test, to fail
 * under some circumstances, like "click on Run all tests" too much.
 * This doesn't mean that S7.Net has bugs, but that the implementation of the
 * server has problems.
 *
 */

//Tests for Async Methods

namespace S7.Net.UnitTest
{
    public partial class S7NetTests
    {
        #region Tests
        [TestMethod]
        public async Task Test_Async_Connection()
        {
            if (plc.IsConnected == false)
            {
                await plc.OpenAsync();
            }
        }

        /// <summary>
        /// Read/Write a single Int16 or UInt16 with a single request.
        /// Test that writing a UInt16 (ushort) and reading it gives the correct value.
        /// Test also that writing a Int16 (short) and reading it gives the correct value.
        /// </summary>
        [TestMethod]
        public async Task Test_Async_WriteAndReadInt16Variable()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            // To write a ushort i don't need any cast, only unboxing must be done
            ushort val = 40000;
            await plc.WriteAsync("DB1.DBW0", val);
            ushort result = (ushort)await plc.ReadAsync("DB1.DBW0");
            Assert.AreEqual(val, result, "A ushort goes from 0 to 64512");

            // To write a short i need to convert it to UShort, then i need to reconvert the readed value to get
            // the negative sign back
            // Depending if i'm writing on a DWORD or on a DEC, i will see ushort or short value in the plc
            short value = -100;
            Assert.IsTrue(plc.IsConnected, "After connecting, IsConnected must be set to true");
            await plc.WriteAsync("DB1.DBW0", value.ConvertToUshort());
            short result2 = ((ushort)await plc.ReadAsync("DB1.DBW0")).ConvertToShort();
            Assert.AreEqual(value, result2, "A short goes from -32767 to 32766");
        }

        /// <summary>
        /// Read/Write a single Int32 or UInt32 with a single request.
        /// Test that writing a UInt32 (uint) and reading it gives the correct value.
        /// Test also that writing a Int32 (int) and reading it gives the correct value.
        /// </summary>
        [TestMethod]
        public async Task Test_Async_WriteAndReadInt32Variable()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            // To write a uint I don't need any cast, only unboxing must be done
            int val = 1000;
            await plc.WriteAsync("DB1.DBD40", val);
            int result = ((uint)await plc.ReadAsync("DB1.DBD40")).ConvertToInt();
            Assert.AreEqual(val, result);

            // To write a int I need to convert it to uint, then I need to reconvert the readed value to get
            // the negative sign back
            // Depending if I'm writing on a DBD or on a LONG, I will see uint or int value in the plc
            int value = -60000;
            await plc.WriteAsync("DB1.DBD60", value);
            int result2 = ((uint)await plc.ReadAsync("DB1.DBD60")).ConvertToInt();
            Assert.AreEqual(value, result2);
        }

        /// <summary>
        /// Read/Write a single REAL with a single request.
        /// Test that writing a float and reading it gives the correct value.
        /// </summary>
        [TestMethod]
        public async Task Test_Async_WriteAndReadRealVariables()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            // Reading and writing a float is quite complicated, because it needs to be converted to DWord before the write,
            // then reconvert to float after the read. Float values can contain only 7 digits, so no precision is lost.
            float val2 = 1234567;
            await plc.WriteAsync("DB1.DBD40", val2.ConvertToUInt());
            float result2 = ((uint)await plc.ReadAsync("DB1.DBD40")).ConvertToFloat();
            Assert.AreEqual(val2, result2);

            float val3 = 12.34567f;
            await plc.WriteAsync("DB1.DBD40", val3.ConvertToUInt());
            float result3 = ((uint)await plc.ReadAsync("DB1.DBD40")).ConvertToFloat();
            Assert.AreEqual(val3, result3);
        }

        /// <summary>
        /// Write/Read a large amount of data to test PDU max
        /// </summary>
        [TestMethod]
        public async Task Test_Async_WriteLargeByteArray()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            var randomEngine = new Random();
            var data = new byte[8192];
            var db = 2;
            randomEngine.NextBytes(data);

            await plc.WriteBytesAsync(DataType.DataBlock, db, 0, data);

            var readData = await plc.ReadBytesAsync(DataType.DataBlock, db, 0, data.Length);

            CollectionAssert.AreEqual(data, readData);
        }

#if NET5_0_OR_GREATER

        /// <summary>
        /// Write/Read a large amount of data to test PDU max
        /// </summary>
        [TestMethod]
        public async Task Test_Async_WriteLargeByteArrayWithMemory()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            var randomEngine = new Random();
            using var dataOwner = MemoryPool<byte>.Shared.Rent(8192);
            var data = dataOwner.Memory.Slice(0, 8192);
            var db = 2;
            randomEngine.NextBytes(data.Span);

            await plc.WriteBytesAsync(DataType.DataBlock, db, 0, data);

            using var readDataOwner = MemoryPool<byte>.Shared.Rent(data.Length);
            var readData = readDataOwner.Memory.Slice(0, data.Length);
            await plc.ReadBytesAsync(readData, DataType.DataBlock, db, 0);

            CollectionAssert.AreEqual(data.ToArray(), readData.ToArray());
        }

#endif

        /// <summary>
        /// Read/Write a class that has the same properties of a DB with the same field in the same order
        /// </summary>
        [TestMethod]
        public async Task Test_Async_ReadAndWriteClass()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestClass tc = new TestClass
            {
                BitVariable00 = true,
                BitVariable10 = true,
                DIntVariable = -100000,
                IntVariable = -15000,
                LRealVariable = -154.789,
                RealVariable = -154.789f,
                DWordVariable = 850,
                WStringVariable = "ÄÜÉÊéà",
                StringVariable = "Hallo"
            };

            await plc.WriteClassAsync(tc, DB2);
            TestClass tc2 = new TestClass();
            // Values that are read from a class are stored inside the class itself, that is passed by reference
            await plc.ReadClassAsync(tc2, DB2);
            Assert.AreEqual(tc.BitVariable00, tc2.BitVariable00);
            Assert.AreEqual(tc.BitVariable10, tc2.BitVariable10);
            Assert.AreEqual(tc.DIntVariable, tc2.DIntVariable);
            Assert.AreEqual(tc.IntVariable, tc2.IntVariable);
            Assert.AreEqual(tc.LRealVariable, tc2.LRealVariable);
            Assert.AreEqual(tc.RealVariable, tc2.RealVariable);
            Assert.AreEqual(tc.DWordVariable, tc2.DWordVariable);
            Assert.AreEqual(tc.WStringVariable, tc2.WStringVariable);
            Assert.AreEqual(tc.StringVariable, tc2.StringVariable);
        }

        [TestMethod]
        public async Task Test_Async_ReadAndWriteNestedClass()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestClassWithNestedClass tc = new TestClassWithNestedClass
            {
                BitVariable00 = true,
                BitVariable01 = new TestClassInnerWithBool { BitVariable00 = true },
                ByteVariable02 = new TestClassInnerWithByte { ByteVariable00 = 128 },
                BitVariable03 = true,
                ShortVariable04 = new TestClassInnerWithShort { ShortVarialbe00 = -15000 }
            };

            await plc.WriteClassAsync(tc, DB4);
            TestClassWithNestedClass tc2 = new TestClassWithNestedClass();
            // Values that are read from a class are stored inside the class itself, that is passed by reference
            await plc.ReadClassAsync(tc2, DB4);
            Assert.AreEqual(tc.BitVariable00, tc2.BitVariable00);
            Assert.AreEqual(tc.BitVariable01.BitVariable00, tc2.BitVariable01.BitVariable00);
            Assert.AreEqual(tc.ByteVariable02.ByteVariable00, tc2.ByteVariable02.ByteVariable00);
            Assert.AreEqual(tc.BitVariable03, tc2.BitVariable03);
            Assert.AreEqual(tc.ShortVariable04.ShortVarialbe00, tc2.ShortVariable04.ShortVarialbe00);
        }

        /// <summary>
        /// Read/Write a struct that has the same properties of a DB with the same field in the same order
        /// </summary>
        [TestMethod]
        public async Task Test_Async_ReadAndWriteStruct()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestStruct tc = new TestStruct
            {
                BitVariable00 = true,
                BitVariable10 = true,
                DIntVariable = -100000,
                IntVariable = -15000,
                LRealVariable = -154.789,
                RealVariable = -154.789f,
                DWordVariable = 850,
                WStringVariable = "ÄÜÉÊéà",
                StringVariable = "Hallo"
            };
            plc.WriteStruct(tc, DB2);
            // Values that are read from a struct are stored in a new struct, returned by the funcion ReadStruct
            TestStruct tc2 = (TestStruct)await plc.ReadStructAsync(typeof(TestStruct), DB2);
            Assert.AreEqual(tc.BitVariable00, tc2.BitVariable00);
            Assert.AreEqual(tc.BitVariable10, tc2.BitVariable10);
            Assert.AreEqual(tc.DIntVariable, tc2.DIntVariable);
            Assert.AreEqual(tc.IntVariable, tc2.IntVariable);
            Assert.AreEqual(tc.LRealVariable, tc2.LRealVariable);
            Assert.AreEqual(tc.RealVariable, tc2.RealVariable);
            Assert.AreEqual(tc.DWordVariable, tc2.DWordVariable);
            Assert.AreEqual(tc.WStringVariable, tc2.WStringVariable);
            Assert.AreEqual(tc.StringVariable, tc2.StringVariable);
        }

        /// <summary>
        /// Read/Write a struct that has the same properties of a DB with the same field in the same order
        /// </summary>
        [TestMethod]
        public async Task Test_Async_ReadAndWriteLongStruct()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestLongStruct tc = new TestLongStruct
            {
                IntVariable0 = 0,
                IntVariable1 = 1,
                IntVariable10 = 10,
                IntVariable11 = 11,
                IntVariable20 = 20,
                IntVariable21 = 21,
                IntVariable30 = 30,
                IntVariable31 = 31,
                IntVariable40 = 40,
                IntVariable41 = 41,
                IntVariable50 = 50,
                IntVariable51 = 51,
                IntVariable60 = 60,
                IntVariable61 = 61,
                IntVariable70 = 70,
                IntVariable71 = 71,
                IntVariable80 = 80,
                IntVariable81 = 81,
                IntVariable90 = 90,
                IntVariable91 = 91,
                IntVariable100 = 100,
                IntVariable101 = 101,
                IntVariable110 = 200,
                IntVariable111 = 201
            };
            plc.WriteStruct(tc, DB2);
            // Values that are read from a struct are stored in a new struct, returned by the funcion ReadStruct
            TestLongStruct tc2 = (TestLongStruct)await plc.ReadStructAsync(typeof(TestLongStruct), DB2);
            Assert.AreEqual(tc.IntVariable0, tc2.IntVariable0);
            Assert.AreEqual(tc.IntVariable1, tc2.IntVariable1);
            Assert.AreEqual(tc.IntVariable10, tc2.IntVariable10);
            Assert.AreEqual(tc.IntVariable11, tc2.IntVariable11);
            Assert.AreEqual(tc.IntVariable20, tc2.IntVariable20);
            Assert.AreEqual(tc.IntVariable21, tc2.IntVariable21);
            Assert.AreEqual(tc.IntVariable30, tc2.IntVariable30);
            Assert.AreEqual(tc.IntVariable31, tc2.IntVariable31);
            Assert.AreEqual(tc.IntVariable40, tc2.IntVariable40);
            Assert.AreEqual(tc.IntVariable41, tc2.IntVariable41);
            Assert.AreEqual(tc.IntVariable50, tc2.IntVariable50);
            Assert.AreEqual(tc.IntVariable51, tc2.IntVariable51);
            Assert.AreEqual(tc.IntVariable60, tc2.IntVariable60);
            Assert.AreEqual(tc.IntVariable61, tc2.IntVariable61);
            Assert.AreEqual(tc.IntVariable70, tc2.IntVariable70);
            Assert.AreEqual(tc.IntVariable71, tc2.IntVariable71);
            Assert.AreEqual(tc.IntVariable80, tc2.IntVariable80);
            Assert.AreEqual(tc.IntVariable81, tc2.IntVariable81);
            Assert.AreEqual(tc.IntVariable90, tc2.IntVariable90);
            Assert.AreEqual(tc.IntVariable91, tc2.IntVariable91);
            Assert.AreEqual(tc.IntVariable100, tc2.IntVariable100);
            Assert.AreEqual(tc.IntVariable101, tc2.IntVariable101);
            Assert.AreEqual(tc.IntVariable110, tc2.IntVariable110);
            Assert.AreEqual(tc.IntVariable111, tc2.IntVariable111);
        }

        /// <summary>
        /// Read/Write a class that has the same properties of a DB with the same field in the same order
        /// </summary>
        [TestMethod]
        public async Task Test_Async_ReadAndWriteLongClass()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestLongClass tc = new TestLongClass
            {
                IntVariable0 = 0,
                IntVariable1 = 1,
                IntVariable10 = 10,
                IntVariable11 = 11,
                IntVariable20 = 20,
                IntVariable21 = 21,
                IntVariable30 = 30,
                IntVariable31 = 31,
                IntVariable40 = 40,
                IntVariable41 = 41,
                IntVariable50 = 50,
                IntVariable51 = 51,
                IntVariable60 = 60,
                IntVariable61 = 61,
                IntVariable70 = 70,
                IntVariable71 = 71,
                IntVariable80 = 80,
                IntVariable81 = 81,
                IntVariable90 = 90,
                IntVariable91 = 91,
                IntVariable100 = 100,
                IntVariable101 = 101,
                IntVariable110 = 200,
                IntVariable111 = 201
            };
            await plc.WriteClassAsync(tc, DB2);
            // Values that are read from a struct are stored in a new struct, returned by the funcion ReadStruct
            TestLongClass tc2 = new TestLongClass();
            await plc.ReadClassAsync(tc2, DB2);
            Assert.AreEqual(tc.IntVariable0, tc2.IntVariable0);
            Assert.AreEqual(tc.IntVariable1, tc2.IntVariable1);
            Assert.AreEqual(tc.IntVariable10, tc2.IntVariable10);
            Assert.AreEqual(tc.IntVariable11, tc2.IntVariable11);
            Assert.AreEqual(tc.IntVariable20, tc2.IntVariable20);
            Assert.AreEqual(tc.IntVariable21, tc2.IntVariable21);
            Assert.AreEqual(tc.IntVariable30, tc2.IntVariable30);
            Assert.AreEqual(tc.IntVariable31, tc2.IntVariable31);
            Assert.AreEqual(tc.IntVariable40, tc2.IntVariable40);
            Assert.AreEqual(tc.IntVariable41, tc2.IntVariable41);
            Assert.AreEqual(tc.IntVariable50, tc2.IntVariable50);
            Assert.AreEqual(tc.IntVariable51, tc2.IntVariable51);
            Assert.AreEqual(tc.IntVariable60, tc2.IntVariable60);
            Assert.AreEqual(tc.IntVariable61, tc2.IntVariable61);
            Assert.AreEqual(tc.IntVariable70, tc2.IntVariable70);
            Assert.AreEqual(tc.IntVariable71, tc2.IntVariable71);
            Assert.AreEqual(tc.IntVariable80, tc2.IntVariable80);
            Assert.AreEqual(tc.IntVariable81, tc2.IntVariable81);
            Assert.AreEqual(tc.IntVariable90, tc2.IntVariable90);
            Assert.AreEqual(tc.IntVariable91, tc2.IntVariable91);
            Assert.AreEqual(tc.IntVariable100, tc2.IntVariable100);
            Assert.AreEqual(tc.IntVariable101, tc2.IntVariable101);
            Assert.AreEqual(tc.IntVariable110, tc2.IntVariable110);
            Assert.AreEqual(tc.IntVariable111, tc2.IntVariable111);
        }

        /// <summary>
        /// Tests that a read and a write on addresses bigger than 8192 are executed correctly
        /// </summary>
        [TestMethod]
        public async Task Test_Async_WriteAndReadInt16VariableAddress8192()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            // To write a ushort i don't need any cast, only unboxing must be done
            ushort val = 8192;
            await plc.WriteAsync("DB2.DBW8192", val);
            ushort result = (ushort)await plc.ReadAsync("DB2.DBW8192");
            Assert.AreEqual(val, result, "A ushort goes from 0 to 64512");

            // To write a short i need to convert it to UShort, then i need to reconvert the readed value to get
            // the negative sign back
            // Depending if i'm writing on a DWORD or on a DEC, i will see ushort or short value in the plc
            short value = -8192;
            Assert.IsTrue(plc.IsConnected, "After connecting, IsConnected must be set to true");
            await plc.WriteAsync("DB2.DBW8192", value.ConvertToUshort());
            short result2 = ((ushort)await plc.ReadAsync("DB2.DBW8192")).ConvertToShort();
            Assert.AreEqual(value, result2, "A short goes from -32767 to 32766");
        }

        /// <summary>
        /// Tests that a read and a write on addresses bigger than 8192 are executed correctly
        /// </summary>
        [TestMethod]
        public async Task Test_Async_WriteAndReadInt16VariableAddress16384()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            // To write a ushort i don't need any cast, only unboxing must be done
            ushort val = 16384;
            await plc.WriteAsync("DB2.DBW16384", val);
            ushort result = (ushort)await plc.ReadAsync("DB2.DBW16384");
            Assert.AreEqual(val, result, "A ushort goes from 0 to 64512");

            // To write a short i need to convert it to UShort, then i need to reconvert the readed value to get
            // the negative sign back
            // Depending if i'm writing on a DWORD or on a DEC, i will see ushort or short value in the plc
            short value = -16384;
            Assert.IsTrue(plc.IsConnected, "After connecting, IsConnected must be set to true");
            await plc.WriteAsync("DB2.DBW16384", value.ConvertToUshort());
            short result2 = ((ushort)await plc.ReadAsync("DB2.DBW16384")).ConvertToShort();
            Assert.AreEqual(value, result2, "A short goes from -32767 to 32766");
        }

        [TestMethod]
        public async Task Test_Async_ReadMultipleBytes()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");
            bool val = true;
            await plc.WriteAsync("DB2.DBX0.5", val);
            bool result = (bool)await plc.ReadAsync("DB2.DBX0.5");
            Assert.AreEqual(val, result);

            ushort val1 = 16384;
            await plc.WriteAsync("DB2.DBW16384", val1);
            ushort result1 = (ushort)await plc.ReadAsync("DB2.DBW16384");
            Assert.AreEqual(val1, result1, "A ushort goes from 0 to 64512");

            bool val2 = true;
            await plc.WriteAsync("DB2.DBX8192.7", val2);
            bool result2 = (bool)await plc.ReadAsync("DB2.DBX8192.7");
            Assert.AreEqual(val2, result2);

            ushort val3 = 129;
            await plc.WriteAsync("DB2.DBW16", val3);
            ushort result3 = (ushort)await plc.ReadAsync("DB2.DBW16");
            Assert.AreEqual(val3, result3, "A ushort goes from 0 to 64512");

            byte[] val4 = new byte[] { 0x12, 0x34 };
            await plc.WriteAsync("DB2.DBB2048", val4[0]);
            await plc.WriteAsync("DB2.DBB2049", val4[1]);
            byte result4b0 = (byte)await plc.ReadAsync("DB2.DBB2048");
            byte result4b1 = (byte)await plc.ReadAsync("DB2.DBB2049");
            Assert.AreEqual(val4[0], result4b0);
            Assert.AreEqual(val4[1], result4b1);

            bool val6 = true;
            await plc.WriteAsync("DB2.DBX16384.6", val6);
            bool result6 = (bool)await plc.ReadAsync("DB2.DBX16384.6");
            Assert.AreEqual(val6, result6);

            var dataItems = new List<DataItem>()
            {
                new DataItem
                {
                    Count = 1,
                    DataType = DataType.DataBlock,
                    DB = 2,
                    StartByteAdr = 0,
                    BitAdr = 5,
                    VarType = VarType.Bit
                }
                ,new DataItem
                {
                    Count = 1,
                    DataType = DataType.DataBlock,
                    DB = 2,
                    StartByteAdr = 16384,
                    VarType = VarType.Word
                },
                new DataItem
                {
                    Count = 1,
                    DataType = DataType.DataBlock,
                    DB = 2,
                    StartByteAdr = 8192,
                    BitAdr = 7,
                    VarType = VarType.Bit
                },
                new DataItem
                {
                    Count = 1,
                    DataType = DataType.DataBlock,
                    DB = 2,
                    StartByteAdr = 16,
                    VarType = VarType.Word
                },
                // single byte
                new DataItem
                {
                    Count = 1,
                    DataType = DataType.DataBlock,
                    DB = 2,
                    StartByteAdr = 2048,
                    VarType = VarType.Byte
                },
                // multiple bytes
                new DataItem
                {
                    Count = 2,
                    DataType = DataType.DataBlock,
                    DB = 2,
                    StartByteAdr = 2048,
                    VarType = VarType.Byte
                },
                new DataItem
                {
                    Count = 1,
                    DataType = DataType.DataBlock,
                    DB = 2,
                    StartByteAdr = 16384,
                    BitAdr = 6,
                    VarType = VarType.Bit
                },
            };

            var dataItemsRes = await plc.ReadMultipleVarsAsync(dataItems);

            Assert.AreEqual(val, dataItemsRes[0].Value);
            Assert.AreEqual(val1, dataItemsRes[1].Value);
            Assert.AreEqual(val2, dataItemsRes[2].Value);
            Assert.AreEqual(val3, dataItemsRes[3].Value);
            Assert.AreEqual(val4[0], dataItemsRes[4].Value);
            Assert.AreEqual(val4[0], ((byte[])dataItemsRes[5].Value)[0]);  //dataItem[5].Value should be byte[2]
            Assert.AreEqual(val4[1], ((byte[])dataItemsRes[5].Value)[1]);
            Assert.AreEqual(val6, dataItemsRes[6].Value);
        }

        /// <summary>
        /// Tests that a read and a write on addresses bigger than 8192 are executed correctly
        /// </summary>
        [TestMethod]
        public async Task Test_Async_WriteAndReadBooleanVariable()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            // tests when writing true/false
            await plc.WriteAsync("DB1.DBX0.0", false);
            var boolVariable = (bool)await plc.ReadAsync("DB1.DBX0.0");
            Assert.IsFalse(boolVariable);

            await plc.WriteAsync("DB1.DBX0.0", true);
            boolVariable = (bool)await plc.ReadAsync("DB1.DBX0.0");
            Assert.IsTrue(boolVariable);

            // tests when writing 0/1
            await plc.WriteAsync("DB1.DBX0.0", 0);
            boolVariable = (bool)await plc.ReadAsync("DB1.DBX0.0");
            Assert.IsFalse(boolVariable);

            await plc.WriteAsync("DB1.DBX0.0", 1);
            boolVariable = (bool)await plc.ReadAsync("DB1.DBX0.0");
            Assert.IsTrue(boolVariable);

            await plc.WriteAsync("DB1.DBX0.7", 1);
            boolVariable = (bool)await plc.ReadAsync("DB1.DBX0.7");
            Assert.IsTrue(boolVariable);

            await plc.WriteAsync("DB1.DBX0.7", 0);
            boolVariable = (bool)await plc.ReadAsync("DB1.DBX0.7");
            Assert.IsFalse(boolVariable);

            await plc.WriteAsync("DB1.DBX658.0", 1);
            boolVariable = (bool)await plc.ReadAsync("DB1.DBX658.0");
            Assert.IsTrue(boolVariable);

            await plc.WriteAsync("DB1.DBX658.7", 1);
            boolVariable = (bool)await plc.ReadAsync("DB1.DBX658.7");
            Assert.IsTrue(boolVariable);

            await plc.WriteAsync("DB2.DBX9658.0", 1);
            boolVariable = (bool)await plc.ReadAsync("DB2.DBX9658.0");
            Assert.IsTrue(boolVariable);
        }

        [TestMethod]
        public async Task Test_Async_ReadClassIgnoresNonPublicSetters()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestClassWithPrivateSetters tc = new TestClassWithPrivateSetters
            {
                BitVariable00 = true,
                BitVariable10 = true,
                DIntVariable = -100000,
                IntVariable = -15000,
                LRealVariable = -154.789,
                RealVariable = -154.789f,
                DWordVariable = 850,
                WStringVariable = "ÄÜÉÊéà",
                StringVariable = "Hallo"
            };

            await plc.WriteClassAsync(tc, DB2);

            TestClassWithPrivateSetters tc2 = new TestClassWithPrivateSetters();
            // Values that are read from a class are stored inside the class itself, that is passed by reference
            var res = await plc.ReadClassAsync(tc2, DB2);
            tc = (TestClassWithPrivateSetters)res.Item2;
            Assert.AreEqual(tc.BitVariable00, tc2.BitVariable00);
            Assert.AreEqual(tc.BitVariable10, tc2.BitVariable10);
            Assert.AreEqual(tc.DIntVariable, tc2.DIntVariable);
            Assert.AreEqual(tc.IntVariable, tc2.IntVariable);
            Assert.AreEqual(tc.LRealVariable, tc2.LRealVariable, 0.1);
            Assert.AreEqual(tc.RealVariable, tc2.RealVariable);
            Assert.AreEqual(tc.DWordVariable, tc2.DWordVariable);

            Assert.AreEqual(TestClassWithPrivateSetters.PRIVATE_SETTER_VALUE, tc2.PrivateSetterProperty);
            Assert.AreEqual(TestClassWithPrivateSetters.PROTECTED_SETTER_VALUE, tc2.ProtectedSetterProperty);
            Assert.AreEqual(TestClassWithPrivateSetters.INTERNAL_SETTER_VALUE, tc2.InternalSetterProperty);
            Assert.AreEqual(TestClassWithPrivateSetters.JUST_A_GETTER_VALUE, tc2.JustAGetterProperty);
        }


        [TestMethod]
        public async Task Test_Async_ReadBytesReturnsNullIfPlcIsNotConnected()
        {
            using (var notConnectedPlc = new Plc(CpuType.S7300, "255.255.255.255", 0, 0))
            {
                Assert.IsFalse(notConnectedPlc.IsConnected);
                TestClass tc = new TestClass();
                await Assert.ThrowsExceptionAsync<PlcException>(async () => await notConnectedPlc.ReadClassAsync(tc, DB2));
            }
        }

        [TestMethod]
        public async Task Test_Async_ReadClassWithGenericReturnsSameResultAsReadClassWithoutGeneric()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestClass tc = new TestClass
            {
                BitVariable00 = true,
                BitVariable10 = true,
                DIntVariable = -100000,
                IntVariable = -15000,
                LRealVariable = -154.789,
                RealVariable = -154.789f,
                DWordVariable = 850,
                WStringVariable = "ÄÜÉÊéà",
                StringVariable = "Hallo"

            };

            await plc.WriteClassAsync(tc, DB2);

            // Values that are read from a class are stored inside the class itself, that is passed by reference
            TestClass tc2 = new TestClass();
            var res = await plc.ReadClassAsync(tc2, DB2);
            tc2 = (TestClass)res.Item2;
            TestClass tc2Generic = await plc.ReadClassAsync<TestClass>(DB2);

            Assert.AreEqual(tc2.BitVariable00, tc2Generic.BitVariable00);
            Assert.AreEqual(tc2.BitVariable10, tc2Generic.BitVariable10);
            Assert.AreEqual(tc2.DIntVariable, tc2Generic.DIntVariable);
            Assert.AreEqual(tc2.IntVariable, tc2Generic.IntVariable);
            Assert.AreEqual(Math.Round(tc2.LRealVariable, 3), Math.Round(tc2Generic.LRealVariable, 3));
            Assert.AreEqual(tc2.RealVariable, tc2Generic.RealVariable);
            Assert.AreEqual(tc2.DWordVariable, tc2Generic.DWordVariable);
            Assert.AreEqual(tc2.WStringVariable, tc2Generic.WStringVariable);
            Assert.AreEqual(tc2.StringVariable, tc2Generic.StringVariable);

        }

        [TestMethod]
        public async Task Test_Async_ReadClassWithGenericReturnsNullIfPlcIsNotConnected()
        {
            using (var notConnectedPlc = new Plc(CpuType.S7300, "255.255.255.255", 0, 0))
            {
                Assert.IsFalse(notConnectedPlc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");
                await Assert.ThrowsExceptionAsync<PlcException>(async () => await notConnectedPlc.ReadClassAsync<TestClass>(DB2));
            }
        }

        [TestMethod]
        public async Task Test_Async_ReadClassWithGenericAndClassFactoryReturnsSameResultAsReadClassWithoutGeneric()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestClass tc = new TestClass
            {
                BitVariable00 = true,
                BitVariable10 = true,
                DIntVariable = -100000,
                IntVariable = -15000,
                LRealVariable = -154.789,
                RealVariable = -154.789f,
                DWordVariable = 850,
                WStringVariable = "ÄÜÉÊéà",
                StringVariable = "Hallo"
            };

            await plc.WriteClassAsync(tc, DB2);

            // Values that are read from a class are stored inside the class itself, that is passed by reference
            TestClass tc2Generic = await plc.ReadClassAsync<TestClass>(DB2);
            TestClass tc2GenericWithClassFactory = await plc.ReadClassAsync(() => new TestClass(), DB2);

            Assert.AreEqual(tc2Generic.BitVariable00, tc2GenericWithClassFactory.BitVariable00);
            Assert.AreEqual(tc2Generic.BitVariable10, tc2GenericWithClassFactory.BitVariable10);
            Assert.AreEqual(tc2Generic.DIntVariable, tc2GenericWithClassFactory.DIntVariable);
            Assert.AreEqual(Math.Round(tc2Generic.LRealVariable, 3), Math.Round(tc2GenericWithClassFactory.LRealVariable, 3));
            Assert.AreEqual(tc2Generic.RealVariable, tc2GenericWithClassFactory.RealVariable);
            Assert.AreEqual(tc2Generic.DWordVariable, tc2GenericWithClassFactory.DWordVariable);
            Assert.AreEqual(tc2Generic.WStringVariable, tc2GenericWithClassFactory.WStringVariable);
            Assert.AreEqual(tc2Generic.StringVariable, tc2GenericWithClassFactory.StringVariable);
        }

        [TestMethod]
        public async Task Test_Async_ReadClassWithGenericAndClassFactoryThrowsExceptionPlcIsNotConnected()
        {
            using (var notConnectedPlc = new Plc(CpuType.S7300, "255.255.255.255", 0, 0))
            {
                Assert.IsFalse(notConnectedPlc.IsConnected);
                await Assert.ThrowsExceptionAsync<PlcException>(async () => await notConnectedPlc.ReadClassAsync(() => new TestClass(), DB2));
            }
        }

        [TestMethod]
        public async Task Test_Async_ReadClassWithNestedClassAfterBit()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            Assert.AreEqual(6, Types.Class.GetClassSize(new TestClassWithNestedClass()));

            TestClassWithNestedClass tc = new TestClassWithNestedClass();
            tc.BitVariable00 = true;
            tc.BitVariable01.BitVariable00 = true;
            tc.ByteVariable02.ByteVariable00 = 128;
            tc.BitVariable03 = true;
            tc.ShortVariable04.ShortVarialbe00 = -15000;

            TestClassWithNestedClass tc2 = await plc.ReadClassAsync<TestClassWithNestedClass>(DB4);
            Assert.AreEqual(tc.BitVariable00, tc2.BitVariable00);
            Assert.AreEqual(tc.BitVariable01.BitVariable00, tc2.BitVariable01.BitVariable00);
            Assert.AreEqual(tc.ByteVariable02.ByteVariable00, tc2.ByteVariable02.ByteVariable00);
            Assert.AreEqual(tc.BitVariable03, tc2.BitVariable03);
            Assert.AreEqual(tc.ShortVariable04.ShortVarialbe00, tc2.ShortVariable04.ShortVarialbe00);
        }

        [TestMethod]
        public async Task Test_Async_ReadStructThrowsExceptionPlcIsNotConnected()
        {
            using (var notConnectedPlc = new Plc(CpuType.S7300, "255.255.255.255", 0, 0))
            {
                Assert.IsFalse(notConnectedPlc.IsConnected);
                await Assert.ThrowsExceptionAsync<PlcException>(async () => await notConnectedPlc.ReadStructAsync(typeof(TestStruct), DB2));
            }
        }

        [TestMethod]
        public async Task Test_Async_ReadStructWithGenericReturnsSameResultAsReadStructWithoutGeneric()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestStruct ts = new TestStruct
            {
                BitVariable00 = true,
                BitVariable10 = true,
                DIntVariable = -100000,
                IntVariable = -15000,
                LRealVariable = -154.789,
                RealVariable = -154.789f,
                DWordVariable = 850,
                WStringVariable = "ÄÜÉÊéà",
                StringVariable = "Hallo"
            };

            plc.WriteStruct(ts, DB2);

            // Values that are read from a struct are stored in a new struct, returned by the funcion ReadStruct
            TestStruct ts2 = (TestStruct)await plc.ReadStructAsync(typeof(TestStruct), DB2);
            var test = await plc.ReadStructAsync<TestStruct>(DB2);
            TestStruct ts2Generic = test.Value;

            Assert.AreEqual(ts2.BitVariable00, ts2Generic.BitVariable00);
            Assert.AreEqual(ts2.BitVariable10, ts2Generic.BitVariable10);
            Assert.AreEqual(ts2.DIntVariable, ts2Generic.DIntVariable);
            Assert.AreEqual(ts2.IntVariable, ts2Generic.IntVariable);
            Assert.AreEqual(ts2.LRealVariable, ts2Generic.LRealVariable);
            Assert.AreEqual(ts2.RealVariable, ts2Generic.RealVariable);
            Assert.AreEqual(ts2.DWordVariable, ts2Generic.DWordVariable);
            Assert.AreEqual(ts2.WStringVariable, ts2Generic.WStringVariable);
            Assert.AreEqual(ts2.StringVariable, ts2Generic.StringVariable);
        }

        [TestMethod]
        public async Task Test_Async_ReadStructWithGenericThrowsExceptionIfPlcIsNotConnected()
        {
            using (var notConnectedPlc = new Plc(CpuType.S7300, "255.255.255.255", 0, 0))
            {
                Assert.IsFalse(notConnectedPlc.IsConnected);
                await Assert.ThrowsExceptionAsync<PlcException>(async () => await notConnectedPlc.ReadStructAsync<TestStruct>(DB2));
            }
        }

        /// <summary>
        /// Tests that the method ReadClass returns the number of bytes read from the plc
        /// </summary>
        [TestMethod]
        public async Task Test_Async_ReadClassReturnsNumberOfReadBytesFromThePlc()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestClass tc = new TestClass
            {
                BitVariable00 = true,
                BitVariable10 = true,
                DIntVariable = -100000,
                IntVariable = -15000,
                LRealVariable = -154.789,
                RealVariable = -154.789f,
                DWordVariable = 850,
                WStringVariable = "ÄÜÉÊéà",
                StringVariable = "Hallo"
            };
            plc.WriteClass(tc, DB2);

            int expectedReadBytes = (int)Types.Class.GetClassSize(tc);

            TestClass tc2 = new TestClass();
            // Values that are read from a class are stored inside the class itself, that is passed by reference
            var res = await plc.ReadClassAsync(tc2, DB2);
            int actualReadBytes = res.Item1;

            Assert.AreEqual(expectedReadBytes, actualReadBytes);
        }

        [TestMethod]
        public async Task Test_Async_ReadClassWithArray()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestClassWithArrays tc = new TestClassWithArrays
            {
                Bool = true
            };
            tc.BoolValues[1] = true;
            tc.Int = int.MinValue;
            tc.Ints[0] = int.MinValue;
            tc.Ints[1] = int.MaxValue;
            tc.Short = short.MinValue;
            tc.Shorts[0] = short.MinValue;
            tc.Shorts[1] = short.MaxValue;
            tc.Double = float.MinValue;
            tc.Doubles[0] = float.MinValue + 1;
            tc.Doubles[1] = float.MaxValue;
            tc.UShort = ushort.MinValue + 1;
            tc.UShorts[0] = ushort.MinValue + 1;
            tc.UShorts[1] = ushort.MaxValue;

            plc.WriteClass(tc, DB2);
            TestClassWithArrays tc2 = await plc.ReadClassAsync<TestClassWithArrays>(DB2);

            Assert.AreEqual(tc.Bool, tc2.Bool);
            Assert.AreEqual(tc.BoolValues[0], tc2.BoolValues[0]);
            Assert.AreEqual(tc.BoolValues[1], tc2.BoolValues[1]);

            Assert.AreEqual(tc.Int, tc2.Int);
            Assert.AreEqual(tc.Ints[0], tc2.Ints[0]);
            Assert.AreEqual(tc.Ints[1], tc.Ints[1]);

            Assert.AreEqual(tc.Short, tc2.Short);
            Assert.AreEqual(tc.Shorts[0], tc2.Shorts[0]);
            Assert.AreEqual(tc.Shorts[1], tc2.Shorts[1]);

            Assert.AreEqual(tc.Double, tc2.Double);
            Assert.AreEqual(tc.Doubles[0], tc2.Doubles[0]);
            Assert.AreEqual(tc.Doubles[1], tc2.Doubles[1]);

            Assert.AreEqual(tc.UShort, tc2.UShort);
            Assert.AreEqual(tc.UShorts[0], tc2.UShorts[0]);
            Assert.AreEqual(tc.UShorts[1], tc2.UShorts[1]);
        }

        [TestMethod]
        public async Task Test_Async_ReadClassWithArrayAndCustomType()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestClassWithCustomType tc = new TestClassWithCustomType
            {
                Int = int.MinValue,
                CustomType = new CustomType()
            };
            tc.CustomType.Bools[1] = true;
            tc.CustomTypes[0] = new CustomType();
            tc.CustomTypes[1] = new CustomType();
            tc.CustomTypes[0].Bools[0] = true;
            tc.CustomTypes[1].Bools[1] = true;

            plc.WriteClass(tc, DB2);
            TestClassWithCustomType tc2 = await plc.ReadClassAsync<TestClassWithCustomType>(DB2);

            Assert.AreEqual(tc.Int, tc2.Int);
            Assert.AreEqual(tc.CustomType.Bools[0], tc2.CustomType.Bools[0]);
            Assert.AreEqual(tc.CustomType.Bools[1], tc2.CustomType.Bools[1]);
            Assert.AreEqual(tc.CustomTypes[0].Bools[0], tc2.CustomTypes[0].Bools[0]);
            Assert.AreEqual(tc.CustomTypes[0].Bools[1], tc2.CustomTypes[0].Bools[1]);
            Assert.AreEqual(tc.CustomTypes[1].Bools[0], tc2.CustomTypes[1].Bools[0]);
            Assert.AreEqual(tc.CustomTypes[1].Bools[1], tc2.CustomTypes[1].Bools[1]);
        }

        [TestMethod]
        public async Task Test_Async_ReadWriteSingle()
        {
            float test_value = 55.6632f;
            await plc.WriteAsync("DB1.DBD0", test_value);
            var helper = await plc.ReadAsync("DB1.DBD0");
            float test_value2 = Conversion.ConvertToFloat((uint)helper);

            Assert.AreEqual(test_value, test_value2, "Compare Write/Read"); //No delta, datatype matches
        }

        [TestMethod]
        public async Task Test_Async_ReadWriteBytesMany()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            var count = 2000;
            var dataItems = new List<byte>();
            for (int i = 0; i < count; i++)
            {
                dataItems.Add((byte)(i % 256));
            }

            await plc.WriteBytesAsync(DataType.DataBlock, 2, 0, dataItems.ToArray());

            var res = await plc.ReadBytesAsync(DataType.DataBlock, 2, 0, count);

            for (int x = 0; x < count; x++)
            {
                Assert.AreEqual(x % 256, res[x], string.Format("Bit {0} failed", x));
            }
        }

#if NET5_0_OR_GREATER

        [TestMethod]
        public async Task Test_Async_ReadWriteBytesManyWithMemory()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            using var data = MemoryPool<byte>.Shared.Rent(2000);
            for (int i = 0; i < data.Memory.Length; i++)
                data.Memory.Span[i] = (byte)(i % 256);

            await plc.WriteBytesAsync(DataType.DataBlock, 2, 0, data.Memory);

            using var readData = MemoryPool<byte>.Shared.Rent(data.Memory.Length);

            await plc.ReadBytesAsync(readData.Memory.Slice(0, data.Memory.Length), DataType.DataBlock, 2, 0);

            for (int x = 0; x < data.Memory.Length; x++)
            {
                Assert.AreEqual(x % 256, readData.Memory.Span[x], string.Format("Bit {0} failed", x));
            }
        }

#endif

        /// <summary>
        /// Write a large amount of data and test cancellation
        /// </summary>
        [TestMethod]
        public async Task Test_Async_WriteLargeByteArrayWithCancellation()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            var cancellationSource = new CancellationTokenSource();
            var cancellationToken = cancellationSource.Token;

            var randomEngine = new Random();
            var data = new byte[8192];
            var db = 2;
            randomEngine.NextBytes(data);

            cancellationSource.CancelAfter(System.TimeSpan.FromMilliseconds(5));
            try
            {
                await plc.WriteBytesAsync(DataType.DataBlock, db, 0, data, cancellationToken);
            }
            catch(OperationCanceledException)
            {
                // everything is good, that is the exception we expect
                Console.WriteLine("Operation was cancelled as expected.");
                return;
            }
            catch(Exception e)
            {
                Assert.Fail($"Wrong exception type received. Expected {typeof(OperationCanceledException)}, received {e.GetType()}.");
            }

            // Depending on how tests run, this can also just succeed without getting cancelled at all. Do nothing in this case.
            Console.WriteLine("Task was not cancelled as expected.");
        }

#if NET5_0_OR_GREATER

        /// <summary>
        /// Write a large amount of data and test cancellation
        /// </summary>
        [TestMethod]
        public async Task Test_Async_WriteLargeByteArrayWithCancellationWithMemory()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            var cancellationSource = new CancellationTokenSource();
            var cancellationToken = cancellationSource.Token;

            using var dataOwner = MemoryPool<byte>.Shared.Rent(8192);
            var data = dataOwner.Memory.Slice(0, 8192);
            var randomEngine = new Random();
            var db = 2;
            randomEngine.NextBytes(data.Span);

            cancellationSource.CancelAfter(System.TimeSpan.FromMilliseconds(5));
            try
            {
                await plc.WriteBytesAsync(DataType.DataBlock, db, 0, data, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // everything is good, that is the exception we expect
                Console.WriteLine("Operation was cancelled as expected.");
                return;
            }
            catch (Exception e)
            {
                Assert.Fail($"Wrong exception type received. Expected {typeof(OperationCanceledException)}, received {e.GetType()}.");
            }

            // Depending on how tests run, this can also just succeed without getting cancelled at all. Do nothing in this case.
            Console.WriteLine("Task was not cancelled as expected.");
        }

#endif

        /// <summary>
        /// Write a large amount of data and test cancellation
        /// </summary>
        [TestMethod]
        public async Task Test_Async_ParseDataIntoDataItemsAlignment()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            var db = 2;
            // First write a sensible S7 string capacity
            await plc.WriteBytesAsync(DataType.DataBlock, db, 0, new byte[] {5, 0});

            // Read two data items, with the first having odd number of bytes (7),
            // and the second has to be aligned on a even address
            var dataItems = new List<DataItem>
            {
                new DataItem
                {
                    DataType = DataType.DataBlock,
                    DB = db,
                    VarType = VarType.S7String,
                    Count = 5
                },
                new DataItem
                {
                    DataType = DataType.DataBlock,
                    DB = db,
                    VarType = VarType.Word,
                }
            };
            await plc.ReadMultipleVarsAsync(dataItems, CancellationToken.None);
        }

        #endregion
    }
}
