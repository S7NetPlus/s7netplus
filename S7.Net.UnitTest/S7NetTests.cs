#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net;
using S7.Net.UnitTest.Helpers;
using S7.Net.UnitTest;
using System.ServiceProcess;
using S7.Net.Types;
using S7.UnitTest.Helpers;

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
namespace S7.Net.UnitTest
{
    [TestClass]
    public class S7NetTests
    {
        #region Constants
        const int DB2 = 2;
        #endregion

        #region Private fields
        Plc plc;
        #endregion

        #region Constructor
        /// <summary>
        /// Create a plc that will connect to localhost (Snap 7 server) and connect to it
        /// </summary>
        public S7NetTests()
        {
            plc = new Plc(CpuType.S7300, "127.0.0.1", 0, 2);
            //ConsoleManager.Show();
            ShutDownServiceS7oiehsx64();

        }

        [TestInitialize]
        public void Setup()
        {
            S7TestServer.Start();
            plc.Open();
        }

        [TestCleanup]
        public void TearDown()
        {
            plc.Close();
            S7TestServer.Stop();
        }

        #endregion

        #region Tests

        [TestMethod]
        public void T00_TestConnection()
        {
            if (plc.IsConnected == false)
            {
                var error = plc.Open();
                Assert.AreEqual(ErrorCode.NoError, error, "If you have s7 installed you must close s7oiehsx64 service.");
            }
        }

        /// <summary>
        /// Read/Write a single Int16 or UInt16 with a single request.
        /// Test that writing a UInt16 (ushort) and reading it gives the correct value.
        /// Test also that writing a Int16 (short) and reading it gives the correct value.
        /// </summary>
        [TestMethod]
        public void T01_WriteAndReadInt16Variable()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            // To write a ushort i don't need any cast, only unboxing must be done
            ushort val = 40000;
            plc.Write("DB1.DBW0", val);
            ushort result = (ushort)plc.Read("DB1.DBW0");
            Assert.AreEqual(val, result, "A ushort goes from 0 to 64512");

            // To write a short i need to convert it to UShort, then i need to reconvert the readed value to get
            // the negative sign back
            // Depending if i'm writing on a DWORD or on a DEC, i will see ushort or short value in the plc
            short value = -100;
            Assert.IsTrue(plc.IsConnected, "After connecting, IsConnected must be set to true");
            plc.Write("DB1.DBW0", value.ConvertToUshort());
            short result2 = ((ushort)plc.Read("DB1.DBW0")).ConvertToShort();
            Assert.AreEqual(value, result2, "A short goes from -32767 to 32766");
        }

        /// <summary>
        /// Read/Write a single Int32 or UInt32 with a single request.
        /// Test that writing a UInt32 (uint) and reading it gives the correct value.
        /// Test also that writing a Int32 (int) and reading it gives the correct value.
        /// </summary>
        [TestMethod]
        public void T02_WriteAndReadInt32Variable()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            // To write a uint I don't need any cast, only unboxing must be done
            uint val = 1000;
            plc.Write("DB1.DBD40", val);
            uint result = (uint)plc.Read("DB1.DBD40");
            Assert.AreEqual(val, result);

            // To write a int I need to convert it to uint, then I need to reconvert the readed value to get
            // the negative sign back
            // Depending if I'm writing on a DBD or on a LONG, I will see uint or int value in the plc
            int value = -60000;
            plc.Write("DB1.DBD60", value);
            int result2 = ((uint)plc.Read("DB1.DBD60")).ConvertToInt();
            Assert.AreEqual(value, result2);
        }

        /// <summary>
        /// Read/Write a single REAL with a single request.
        /// Test that writing a double and reading it gives the correct value.
        /// </summary>
        [TestMethod]
        public void T03_WriteAndReadRealVariables()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            // Reading and writing a double is quite complicated, because it needs to be converted to DWord before the write,
            // then reconvert to double after the read.
            double val = 35.687;
            plc.Write("DB1.DBD40", val.ConvertToUInt());
            double result = ((uint)plc.Read("DB1.DBD40")).ConvertToDouble();
            Assert.AreEqual(val, Math.Round(result, 3)); // float lose precision, so i need to round it
        }

        /// <summary>
        /// Read/Write a class that has the same properties of a DB with the same field in the same order
        /// </summary>
        [TestMethod]
        public void T04_ReadAndWriteClass()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestClass tc = new TestClass();
            tc.BitVariable00 = true;
            tc.BitVariable10 = true;
            tc.DIntVariable = -100000;
            tc.IntVariable = -15000;
            tc.RealVariable = -154.789;
            tc.DWordVariable = 850;
            plc.WriteClass(tc, DB2);
            TestClass tc2 = new TestClass();
            // Values that are read from a class are stored inside the class itself, that is passed by reference
            plc.ReadClass(tc2, DB2);
            Assert.AreEqual(tc.BitVariable00, tc2.BitVariable00);
            Assert.AreEqual(tc.BitVariable10, tc2.BitVariable10);
            Assert.AreEqual(tc.DIntVariable, tc2.DIntVariable);
            Assert.AreEqual(tc.IntVariable, tc2.IntVariable);
            Assert.AreEqual(tc.RealVariable, Math.Round(tc2.RealVariable, 3));
            Assert.AreEqual(tc.DWordVariable, tc2.DWordVariable);
        }

        /// <summary>
        /// Read/Write a struct that has the same properties of a DB with the same field in the same order
        /// </summary>
        [TestMethod]
        public void T05_ReadAndWriteStruct()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestStruct tc = new TestStruct();
            tc.BitVariable00 = true;
            tc.BitVariable10 = true;
            tc.DIntVariable = -100000;
            tc.IntVariable = -15000;
            tc.RealVariable = -154.789;
            tc.DWordVariable = 850;
            plc.WriteStruct(tc, DB2);
            // Values that are read from a struct are stored in a new struct, returned by the funcion ReadStruct
            TestStruct tc2 = (TestStruct)plc.ReadStruct(typeof(TestStruct), DB2);
            Assert.AreEqual(tc.BitVariable00, tc2.BitVariable00);
            Assert.AreEqual(tc.BitVariable10, tc2.BitVariable10);
            Assert.AreEqual(tc.DIntVariable, tc2.DIntVariable);
            Assert.AreEqual(tc.IntVariable, tc2.IntVariable);
            Assert.AreEqual(tc.RealVariable, Math.Round(tc2.RealVariable, 3));
            Assert.AreEqual(tc.DWordVariable, tc2.DWordVariable);
        }

        /// <summary>
        /// Read/Write a struct that has the same properties of a DB with the same field in the same order
        /// </summary>
        [TestMethod]
        public void T06_ReadAndWriteLongStruct()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestLongStruct tc = new TestLongStruct();
            tc.IntVariable0 = 0;
            tc.IntVariable1 = 1;
            tc.IntVariable10 = 10;
            tc.IntVariable11 = 11;
            tc.IntVariable20 = 20;
            tc.IntVariable21 = 21;
            tc.IntVariable30 = 30;
            tc.IntVariable31 = 31;
            tc.IntVariable40 = 40;
            tc.IntVariable41 = 41;
            tc.IntVariable50 = 50;
            tc.IntVariable51 = 51;
            tc.IntVariable60 = 60;
            tc.IntVariable61 = 61;
            tc.IntVariable70 = 70;
            tc.IntVariable71 = 71;
            tc.IntVariable80 = 80;
            tc.IntVariable81 = 81;
            tc.IntVariable90 = 90;
            tc.IntVariable91 = 91;
            tc.IntVariable100 = 100;
            tc.IntVariable101 = 101;
            tc.IntVariable110 = 200;
            tc.IntVariable111 = 201;
            plc.WriteStruct(tc, DB2);
            Assert.AreEqual(ErrorCode.NoError, plc.LastErrorCode);
            // Values that are read from a struct are stored in a new struct, returned by the funcion ReadStruct
            TestLongStruct tc2 = (TestLongStruct)plc.ReadStruct(typeof(TestLongStruct), DB2);
            Assert.AreEqual(ErrorCode.NoError, plc.LastErrorCode);
            Assert.AreEqual( tc.IntVariable0,     tc2.IntVariable0 );
            Assert.AreEqual( tc.IntVariable1,     tc2.IntVariable1 );
            Assert.AreEqual( tc.IntVariable10, tc2.IntVariable10);
            Assert.AreEqual( tc.IntVariable11, tc2.IntVariable11);
            Assert.AreEqual( tc.IntVariable20, tc2.IntVariable20);
            Assert.AreEqual( tc.IntVariable21, tc2.IntVariable21);
            Assert.AreEqual( tc.IntVariable30, tc2.IntVariable30);
            Assert.AreEqual( tc.IntVariable31, tc2.IntVariable31);
            Assert.AreEqual( tc.IntVariable40, tc2.IntVariable40);
            Assert.AreEqual( tc.IntVariable41, tc2.IntVariable41);
            Assert.AreEqual( tc.IntVariable50, tc2.IntVariable50);
            Assert.AreEqual( tc.IntVariable51, tc2.IntVariable51);
            Assert.AreEqual( tc.IntVariable60, tc2.IntVariable60);
            Assert.AreEqual( tc.IntVariable61, tc2.IntVariable61);
            Assert.AreEqual( tc.IntVariable70, tc2.IntVariable70);
            Assert.AreEqual( tc.IntVariable71, tc2.IntVariable71);
            Assert.AreEqual( tc.IntVariable80, tc2.IntVariable80);
            Assert.AreEqual( tc.IntVariable81, tc2.IntVariable81);
            Assert.AreEqual( tc.IntVariable90, tc2.IntVariable90);
            Assert.AreEqual(tc.IntVariable91,  tc2.IntVariable91);
            Assert.AreEqual(tc.IntVariable100, tc2.IntVariable100);
            Assert.AreEqual(tc.IntVariable101, tc2.IntVariable101);
            Assert.AreEqual(tc.IntVariable110, tc2.IntVariable110);
            Assert.AreEqual(tc.IntVariable111, tc2.IntVariable111);
        }

        /// <summary>
        /// Read/Write a class that has the same properties of a DB with the same field in the same order
        /// </summary>
        [TestMethod]
        public void T07_ReadAndWriteLongClass()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestLongClass tc = new TestLongClass();
            tc.IntVariable0 = 0;
            tc.IntVariable1 = 1;
            tc.IntVariable10 = 10;
            tc.IntVariable11 = 11;
            tc.IntVariable20 = 20;
            tc.IntVariable21 = 21;
            tc.IntVariable30 = 30;
            tc.IntVariable31 = 31;
            tc.IntVariable40 = 40;
            tc.IntVariable41 = 41;
            tc.IntVariable50 = 50;
            tc.IntVariable51 = 51;
            tc.IntVariable60 = 60;
            tc.IntVariable61 = 61;
            tc.IntVariable70 = 70;
            tc.IntVariable71 = 71;
            tc.IntVariable80 = 80;
            tc.IntVariable81 = 81;
            tc.IntVariable90 = 90;
            tc.IntVariable91 = 91;
            tc.IntVariable100 = 100;
            tc.IntVariable101 = 101;
            tc.IntVariable110 = 200;
            tc.IntVariable111 = 201;
            plc.WriteClass(tc, DB2);
            Assert.AreEqual(ErrorCode.NoError, plc.LastErrorCode);
            // Values that are read from a struct are stored in a new struct, returned by the funcion ReadStruct
            TestLongClass tc2 = new TestLongClass();
            plc.ReadClass(tc2, DB2);
            Assert.AreEqual(ErrorCode.NoError, plc.LastErrorCode);
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
        public void T08_WriteAndReadInt16VariableAddress8192()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            // To write a ushort i don't need any cast, only unboxing must be done
            ushort val = 8192;
            plc.Write("DB2.DBW8192", val);
            ushort result = (ushort)plc.Read("DB2.DBW8192");
            Assert.AreEqual(val, result, "A ushort goes from 0 to 64512");

            // To write a short i need to convert it to UShort, then i need to reconvert the readed value to get
            // the negative sign back
            // Depending if i'm writing on a DWORD or on a DEC, i will see ushort or short value in the plc
            short value = -8192;
            Assert.IsTrue(plc.IsConnected, "After connecting, IsConnected must be set to true");
            plc.Write("DB2.DBW8192", value.ConvertToUshort());
            short result2 = ((ushort)plc.Read("DB2.DBW8192")).ConvertToShort();
            Assert.AreEqual(value, result2, "A short goes from -32767 to 32766");
        }

        /// <summary>
        /// Tests that a read and a write on addresses bigger than 8192 are executed correctly
        /// </summary>
        [TestMethod]
        public void T09_WriteAndReadInt16VariableAddress16384()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            // To write a ushort i don't need any cast, only unboxing must be done
            ushort val = 16384;
            plc.Write("DB2.DBW16384", val);
            ushort result = (ushort)plc.Read("DB2.DBW16384");
            Assert.AreEqual(val, result, "A ushort goes from 0 to 64512");

            // To write a short i need to convert it to UShort, then i need to reconvert the readed value to get
            // the negative sign back
            // Depending if i'm writing on a DWORD or on a DEC, i will see ushort or short value in the plc
            short value = -16384;
            Assert.IsTrue(plc.IsConnected, "After connecting, IsConnected must be set to true");
            plc.Write("DB2.DBW16384", value.ConvertToUshort());
            short result2 = ((ushort)plc.Read("DB2.DBW16384")).ConvertToShort();
            Assert.AreEqual(value, result2, "A short goes from -32767 to 32766");
        }

        [TestMethod]
        public void T10_ReadMultipleBytes()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            ushort val = 16384;
            plc.Write("DB2.DBW16384", val);
            ushort result = (ushort)plc.Read("DB2.DBW16384");
            Assert.AreEqual(val, result, "A ushort goes from 0 to 64512");

            ushort val2 = 129;
            plc.Write("DB2.DBW16", val2);
            ushort result2 = (ushort)plc.Read("DB2.DBW16");
            Assert.AreEqual(val2, result2, "A ushort goes from 0 to 64512");

            var dataItems = new List<DataItem>()
            {
                new DataItem
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
                    StartByteAdr = 16,
                    VarType = VarType.Word
                }
            };

            plc.ReadMultipleVars(dataItems);

            Assert.AreEqual(dataItems[0].Value, val);
            Assert.AreEqual(dataItems[1].Value, val2);
        }

        /// <summary>
        /// Tests that a read and a write on addresses bigger than 8192 are executed correctly
        /// </summary>
        [TestMethod]
        public void T11_WriteAndReadBooleanVariable()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            // tests when writing true/false
            plc.Write("DB1.DBX0.0", false);
            var boolVariable = (bool)plc.Read("DB1.DBX0.0");
            Assert.IsFalse(boolVariable);

            plc.Write("DB1.DBX0.0", true);
            boolVariable = (bool)plc.Read("DB1.DBX0.0");
            Assert.IsTrue(boolVariable);

            // tests when writing 0/1
            plc.Write("DB1.DBX0.0", 0);
            boolVariable = (bool)plc.Read("DB1.DBX0.0");
            Assert.IsFalse(boolVariable);

            plc.Write("DB1.DBX0.0", 1);
            boolVariable = (bool)plc.Read("DB1.DBX0.0");
            Assert.IsTrue(boolVariable);

            plc.Write("DB1.DBX0.7", 1);
            boolVariable = (bool)plc.Read("DB1.DBX0.7");
            Assert.IsTrue(boolVariable);

            plc.Write("DB1.DBX0.7", 0);
            boolVariable = (bool)plc.Read("DB1.DBX0.7");
            Assert.IsFalse(boolVariable);

            plc.Write("DB1.DBX658.0", 1);
            boolVariable = (bool)plc.Read("DB1.DBX658.0");
            Assert.IsTrue(boolVariable);

            plc.Write("DB1.DBX658.7", 1);
            boolVariable = (bool)plc.Read("DB1.DBX658.7");
            Assert.IsTrue(boolVariable);

            plc.Write("DB2.DBX9658.0", 1);
            boolVariable = (bool)plc.Read("DB2.DBX9658.0");
            Assert.IsTrue(boolVariable);
        }

        [TestMethod]
        public void T12_ReadClassIgnoresNonPublicSetters()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestClassWithPrivateSetters tc = new TestClassWithPrivateSetters();
            tc.BitVariable00 = true;
            tc.BitVariable10 = true;
            tc.DIntVariable = -100000;
            tc.IntVariable = -15000;
            tc.RealVariable = -154.789;
            tc.DWordVariable = 850;

            plc.WriteClass(tc, DB2);

            TestClassWithPrivateSetters tc2 = new TestClassWithPrivateSetters();
            // Values that are read from a class are stored inside the class itself, that is passed by reference
            plc.ReadClass(tc2, DB2);
            Assert.AreEqual(tc.BitVariable00, tc2.BitVariable00);
            Assert.AreEqual(tc.BitVariable10, tc2.BitVariable10);
            Assert.AreEqual(tc.DIntVariable, tc2.DIntVariable);
            Assert.AreEqual(tc.IntVariable, tc2.IntVariable);
            Assert.AreEqual(tc.RealVariable, Math.Round(tc2.RealVariable, 3));
            Assert.AreEqual(tc.DWordVariable, tc2.DWordVariable);

            Assert.AreEqual(TestClassWithPrivateSetters.PRIVATE_SETTER_VALUE, tc2.PrivateSetterProperty);
            Assert.AreEqual(TestClassWithPrivateSetters.PROTECTED_SETTER_VALUE, tc2.ProtectedSetterProperty);
            Assert.AreEqual(TestClassWithPrivateSetters.INTERNAL_SETTER_VALUE, tc2.InternalSetterProperty);
            Assert.AreEqual(TestClassWithPrivateSetters.JUST_A_GETTER_VALUE, tc2.JustAGetterProperty);
        }


        [TestMethod]
        public void T13_ReadBytesReturnsEmptyArrayIfPlcIsNotConnected()
        {
            using (var notConnectedPlc = new Plc(CpuType.S7300, "255.255.255.255", 0, 0))
            {
                Assert.IsFalse(notConnectedPlc.IsConnected);

                int expectedReadBytes = 0; // 0 bytes, because no connection was established

                TestClass tc = new TestClass();
                int actualReadBytes = notConnectedPlc.ReadClass(tc, DB2);

                Assert.AreEqual(expectedReadBytes, actualReadBytes);
            }
        }

        [TestMethod]
        public void T14_ReadClassWithGenericReturnsSameResultAsReadClassWithoutGeneric()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestClass tc = new TestClass();
            tc.BitVariable00 = true;
            tc.BitVariable10 = true;
            tc.DIntVariable = -100000;
            tc.IntVariable = -15000;
            tc.RealVariable = -154.789;
            tc.DWordVariable = 850;

            plc.WriteClass(tc, DB2);

            // Values that are read from a class are stored inside the class itself, that is passed by reference
            TestClass tc2 = new TestClass();
            plc.ReadClass(tc2, DB2);
            TestClass tc2Generic = plc.ReadClass<TestClass>(DB2);

            Assert.AreEqual(tc2.BitVariable00, tc2Generic.BitVariable00);
            Assert.AreEqual(tc2.BitVariable10, tc2Generic.BitVariable10);
            Assert.AreEqual(tc2.DIntVariable, tc2Generic.DIntVariable);
            Assert.AreEqual(tc2.IntVariable, tc2Generic.IntVariable);
            Assert.AreEqual(Math.Round(tc2.RealVariable, 3), Math.Round(tc2Generic.RealVariable, 3));
            Assert.AreEqual(tc2.DWordVariable, tc2Generic.DWordVariable);
        }

        [TestMethod]
        public void T15_ReadClassWithGenericReturnsNullIfPlcIsNotConnected()
        {
            using (var notConnectedPlc = new Plc(CpuType.S7300, "255.255.255.255", 0, 0))
            {
                Assert.IsFalse(notConnectedPlc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

                TestClass tc = notConnectedPlc.ReadClass<TestClass>(DB2);

                Assert.IsNull(tc);
            }
        }

        [TestMethod]
        public void T16_ReadClassWithGenericAndClassFactoryReturnsSameResultAsReadClassWithoutGeneric()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestClass tc = new TestClass();
            tc.BitVariable00 = true;
            tc.BitVariable10 = true;
            tc.DIntVariable = -100000;
            tc.IntVariable = -15000;
            tc.RealVariable = -154.789;
            tc.DWordVariable = 850;

            plc.WriteClass(tc, DB2);

            // Values that are read from a class are stored inside the class itself, that is passed by reference
            TestClass tc2Generic = plc.ReadClass<TestClass>(DB2);
            TestClass tc2GenericWithClassFactory = plc.ReadClass(() => new TestClass(), DB2);

            Assert.AreEqual(tc2Generic.BitVariable00, tc2GenericWithClassFactory.BitVariable00);
            Assert.AreEqual(tc2Generic.BitVariable10, tc2GenericWithClassFactory.BitVariable10);
            Assert.AreEqual(tc2Generic.DIntVariable, tc2GenericWithClassFactory.DIntVariable);
            Assert.AreEqual(tc2Generic.IntVariable, tc2GenericWithClassFactory.IntVariable);
            Assert.AreEqual(Math.Round(tc2Generic.RealVariable, 3), Math.Round(tc2GenericWithClassFactory.RealVariable, 3));
            Assert.AreEqual(tc2Generic.DWordVariable, tc2GenericWithClassFactory.DWordVariable);
        }

        [TestMethod]
        public void T17_ReadClassWithGenericAndClassFactoryReturnsNullIfPlcIsNotConnected()
        {
            using (var notConnectedPlc = new Plc(CpuType.S7300, "255.255.255.255", 0, 0))
            {
                Assert.IsFalse(notConnectedPlc.IsConnected);

                TestClass tc = notConnectedPlc.ReadClass(() => new TestClass(), DB2);

                Assert.IsNull(tc);
            }
        }

        [TestMethod]
        public void T18_ReadStructReturnsNullIfPlcIsNotConnected()
        {
            using (var notConnectedPlc = new Plc(CpuType.S7300, "255.255.255.255", 0, 0))
            {
                Assert.IsFalse(notConnectedPlc.IsConnected);

                object tsObj = notConnectedPlc.ReadStruct(typeof(TestStruct), DB2);

                Assert.IsNull(tsObj);
            }
        }

        [TestMethod]
        public void T19_ReadStructWithGenericReturnsSameResultAsReadStructWithoutGeneric()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestStruct ts = new TestStruct();
            ts.BitVariable00 = true;
            ts.BitVariable10 = true;
            ts.DIntVariable = -100000;
            ts.IntVariable = -15000;
            ts.RealVariable = -154.789;
            ts.DWordVariable = 850;

            plc.WriteStruct(ts, DB2);

            // Values that are read from a struct are stored in a new struct, returned by the funcion ReadStruct
            TestStruct ts2 = (TestStruct)plc.ReadStruct(typeof(TestStruct), DB2);
            TestStruct ts2Generic = plc.ReadStruct<TestStruct>(DB2).Value;

            Assert.AreEqual(ts2.BitVariable00, ts2Generic.BitVariable00);
            Assert.AreEqual(ts2.BitVariable10, ts2Generic.BitVariable10);
            Assert.AreEqual(ts2.DIntVariable, ts2Generic.DIntVariable);
            Assert.AreEqual(ts2.IntVariable, ts2Generic.IntVariable);
            Assert.AreEqual(Math.Round(ts2.RealVariable, 3), Math.Round(ts2Generic.RealVariable, 3));
            Assert.AreEqual(ts2.DWordVariable, ts2Generic.DWordVariable);
        }

        [TestMethod]
        public void T20_ReadStructWithGenericReturnsNullIfPlcIsNotConnected()
        {
            using (var notConnectedPlc = new Plc(CpuType.S7300, "255.255.255.255", 0, 0))
            {
                Assert.IsFalse(notConnectedPlc.IsConnected);

                object tsObj = notConnectedPlc.ReadStruct<TestStruct>(DB2);

                Assert.IsNull(tsObj);
            }
        }

        /// <summary>
        /// Tests that the method ReadClass returns the number of bytes read from the plc
        /// </summary>
        [TestMethod]
        public void T21_ReadClassReturnsNumberOfReadBytesFromThePlc()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestClass tc = new TestClass();
            tc.BitVariable00 = true;
            tc.BitVariable10 = true;
            tc.DIntVariable = -100000;
            tc.IntVariable = -15000;
            tc.RealVariable = -154.789;
            tc.DWordVariable = 850;
            plc.WriteClass(tc, DB2);

            int expectedReadBytes = Types.Class.GetClassSize(tc);

            TestClass tc2 = new TestClass();
            // Values that are read from a class are stored inside the class itself, that is passed by reference
            int actualReadBytes = plc.ReadClass(tc2, DB2);

            Assert.AreEqual(expectedReadBytes, actualReadBytes);
        }
        
        [TestMethod]
        public void T22_ReadClassWithArray()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestClassWithArrays tc = new TestClassWithArrays();
            tc.Bool = true;
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
            TestClassWithArrays tc2 = plc.ReadClass<TestClassWithArrays>(DB2);

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
        public void T23_ReadClassWithArrayAndCustomType()
        {
            Assert.IsTrue(plc.IsConnected, "Before executing this test, the plc must be connected. Check constructor.");

            TestClassWithCustomType tc = new TestClassWithCustomType();
            tc.Int = int.MinValue;
            tc.CustomType = new CustomType();
            tc.CustomType.Bools[1] = true;
            tc.CustomTypes[0] = new CustomType();
            tc.CustomTypes[1] = new CustomType();
            tc.CustomTypes[0].Bools[0] = true;
            tc.CustomTypes[1].Bools[1] = true;

            plc.WriteClass(tc, DB2);
            TestClassWithCustomType tc2 = plc.ReadClass<TestClassWithCustomType>(DB2);

            Assert.AreEqual(tc.Int, tc2.Int);
            Assert.AreEqual(tc.CustomType.Bools[0], tc2.CustomType.Bools[0]);
            Assert.AreEqual(tc.CustomType.Bools[1], tc2.CustomType.Bools[1]);
            Assert.AreEqual(tc.CustomTypes[0].Bools[0], tc2.CustomTypes[0].Bools[0]);
            Assert.AreEqual(tc.CustomTypes[0].Bools[1], tc2.CustomTypes[0].Bools[1]);
            Assert.AreEqual(tc.CustomTypes[1].Bools[0], tc2.CustomTypes[1].Bools[0]);
            Assert.AreEqual(tc.CustomTypes[1].Bools[1], tc2.CustomTypes[1].Bools[1]);
        }

        [TestMethod]
        public void T24_IsAvailableReturnsFalseIfIPAddressIsNotReachable()
        {
            plc.Close();
            S7TestServer.Stop();

            var unreachablePlc = new Plc(CpuType.S7300, "255.255.255.255", 0, 2);
            Assert.IsFalse(unreachablePlc.IsAvailable);
        }

        [TestMethod]
        public void T25_IsAvailableReturnsTrueIfIPAddressIsReachable()
        {
            plc.Close();
            S7TestServer.Stop();
            S7TestServer.Start();

            var reachablePlc = new Plc(CpuType.S7300, "127.0.0.1", 0, 2);
            Assert.IsTrue(reachablePlc.IsAvailable);
        }

        #endregion

        #region Private methods

        private static void ShutDownServiceS7oiehsx64()
        {
            try
            {
                ServiceController sc = new ServiceController("s7oiehsx64");
                switch (sc.Status)
                {
                    case ServiceControllerStatus.Running:
                        sc.Stop();
                        break;
                }
            }
            catch { } // service not found
        }

        #endregion
    }
}
