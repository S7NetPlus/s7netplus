#region Using
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net;
using S7.Net.UnitTest.Helpers;
using S7.Net.UnitTest;
using System.ServiceProcess; 

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
            S7TestServer.Start();
            plc.Open();
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
