using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace S7.Net.UnitTest.TypeTests
{
    public static class DateTimeTests
    {
        private static readonly DateTime SampleDateTime = new DateTime(1993, 12, 25, 8, 12, 34, 567);

        private static readonly byte[] SampleByteArray = {0x93, 0x12, 0x25, 0x08, 0x12, 0x34, 0x56, 7 << 4 | 7};

        private static readonly byte[] SpecMinByteArray =
        {
            0x90, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, (byte) (int) (Types.DateTime.SpecMinimumDateTime.DayOfWeek + 1)
        };

        private static readonly byte[] SpecMaxByteArray =
        {
            0x89, 0x12, 0x31, 0x23, 0x59, 0x59, 0x99, (byte) (9 << 4 | (int) (Types.DateTime.SpecMaximumDateTime.DayOfWeek + 1))
        };

        [TestClass]
        public class FromByteArray
        {
            [TestMethod]
            public void Sample()
            {
                AssertFromByteArrayEquals(SampleDateTime, SampleByteArray);
            }

            [TestMethod]
            public void SpecMinimum()
            {
                AssertFromByteArrayEquals(Types.DateTime.SpecMinimumDateTime, SpecMinByteArray);
            }

            [TestMethod]
            public void SpecMaximum()
            {
                AssertFromByteArrayEquals(Types.DateTime.SpecMaximumDateTime, SpecMaxByteArray);
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnLessThan8Bytes()
            {
                Types.DateTime.FromByteArray(new byte[7]);
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnMoreTHan8Bytes()
            {
                Types.DateTime.FromByteArray(new byte[9]);
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnInvalidYear()
            {
                Types.DateTime.FromByteArray(MutateSample(0, 0xa0));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnZeroMonth()
            {
                Types.DateTime.FromByteArray(MutateSample(1, 0x00));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnTooLargeMonth()
            {
                Types.DateTime.FromByteArray(MutateSample(1, 0x13));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnZeroDay()
            {
                Types.DateTime.FromByteArray(MutateSample(2, 0x00));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnTooLargeDay()
            {
                Types.DateTime.FromByteArray(MutateSample(2, 0x32));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnInvalidHour()
            {
                Types.DateTime.FromByteArray(MutateSample(3, 0x24));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnInvalidMinute()
            {
                Types.DateTime.FromByteArray(MutateSample(4, 0x60));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnInvalidSecond()
            {
                Types.DateTime.FromByteArray(MutateSample(5, 0x60));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnInvalidFirstTwoMillisecondDigits()
            {
                Types.DateTime.FromByteArray(MutateSample(6, 0xa0));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnInvalidThirdMillisecondDigit()
            {
                Types.DateTime.FromByteArray(MutateSample(7, 10 << 4));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnZeroDayOfWeek()
            {
                Types.DateTime.FromByteArray(MutateSample(7, 0));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnTooLargeDayOfWeek()
            {
                Types.DateTime.FromByteArray(MutateSample(7, 8));
            }

            private static void AssertFromByteArrayEquals(DateTime expected, params byte[] bytes)
            {
                Assert.AreEqual(expected, Types.DateTime.FromByteArray(bytes));
            }

            private static byte[] MutateSample(int index, byte value) =>
                SampleByteArray.Select((b, i) => i == index ? value : b).ToArray();
        }

        [TestClass]
        public class ToByteArray
        {
            [TestMethod]
            public void Sample()
            {
                AssertToByteArrayEquals(SampleDateTime, SampleByteArray);
            }

            [TestMethod]
            public void SpecMinimum()
            {
                AssertToByteArrayEquals(Types.DateTime.SpecMinimumDateTime, SpecMinByteArray);
            }

            [TestMethod]
            public void SpecMaximum()
            {
                AssertToByteArrayEquals(Types.DateTime.SpecMaximumDateTime, SpecMaxByteArray);
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnTimeBeforeSpecMinimum()
            {
                Types.DateTime.ToByteArray(new DateTime(1970, 1, 1));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnTimeAfterSpecMaximum()
            {
                Types.DateTime.ToByteArray(new DateTime(2090, 1, 1));
            }

            private static void AssertToByteArrayEquals(DateTime value, params byte[] expected)
            {
                CollectionAssert.AreEqual(expected, Types.DateTime.ToByteArray(value));
            }
        }
    }
}
