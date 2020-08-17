using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace S7.Net.UnitTest.TypeTests
{
    public static class DateTimeLongTests
    {
        private static readonly DateTime SampleDateTime = new DateTime(1993, 12, 25, 8, 12, 34, 567);

        private static readonly byte[] SampleByteArray = {0x07, 0xC9, 0x0C, 0x19, 0x07, 0x08, 0x0C, 0x22, 0x21, 0xCB, 0xBB, 0xC0 };

        private static readonly byte[] SpecMinByteArray =
        {
            0x07, 0xB2, 0x01, 0x01,  (byte) (int) (Types.DateTimeLong.SpecMinimumDateTime.DayOfWeek + 1), 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        private static readonly byte[] SpecMaxByteArray =
        {
            0x08, 0xD6, 0x04, 0x0B,  (byte) (int) (Types.DateTimeLong.SpecMaximumDateTime.DayOfWeek + 1), 0x17, 0x2F, 0x10, 0x32, 0xE7, 0x01, 0x80
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
                AssertFromByteArrayEquals(Types.DateTimeLong.SpecMinimumDateTime, SpecMinByteArray);
            }

            [TestMethod]
            public void SpecMaximum()
            {
                AssertFromByteArrayEquals(Types.DateTimeLong.SpecMaximumDateTime, SpecMaxByteArray);
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnLessThan12Bytes()
            {
                Types.DateTimeLong.FromByteArray(new byte[11]);
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnMoreTHan12Bytes()
            {
                Types.DateTimeLong.FromByteArray(new byte[13]);
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnInvalidYear()
            {
                Types.DateTimeLong.FromByteArray(MutateSample(0, 0xa0));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnZeroMonth()
            {
                Types.DateTimeLong.FromByteArray(MutateSample(2, 0x00));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnTooLargeMonth()
            {
                Types.DateTimeLong.FromByteArray(MutateSample(2, 0x13));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnZeroDay()
            {
                Types.DateTimeLong.FromByteArray(MutateSample(3, 0x00));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnTooLargeDay()
            {
                Types.DateTimeLong.FromByteArray(MutateSample(3, 0x32));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnInvalidHour()
            {
                Types.DateTimeLong.FromByteArray(MutateSample(5, 0x24));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnInvalidMinute()
            {
                Types.DateTimeLong.FromByteArray(MutateSample(6, 0x60));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnInvalidSecond()
            {
                Types.DateTimeLong.FromByteArray(MutateSample(7, 0x60));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnInvalidNanosecondsFirstDigit()
            {
                Types.DateTimeLong.FromByteArray(MutateSample(8, 0x3B));
            }


            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnZeroDayOfWeek()
            {
                Types.DateTimeLong.FromByteArray(MutateSample(4, 0));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnTooLargeDayOfWeek()
            {
                Types.DateTimeLong.FromByteArray(MutateSample(4, 8));
            }

            private static void AssertFromByteArrayEquals(DateTime expected, params byte[] bytes)
            {
                Assert.AreEqual(expected, Types.DateTimeLong.FromByteArray(bytes));
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
                AssertToByteArrayEquals(Types.DateTimeLong.SpecMinimumDateTime, SpecMinByteArray);
            }

            [TestMethod]
            public void SpecMaximum()
            {
                AssertToByteArrayEquals(Types.DateTimeLong.SpecMaximumDateTime, SpecMaxByteArray);
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnTimeBeforeSpecMinimum()
            {
                Types.DateTimeLong.ToByteArray(new DateTime(1950, 1, 1));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnTimeAfterSpecMaximum()
            {
                Types.DateTimeLong.ToByteArray(new DateTime(2790, 1, 1));
            }

            private static void AssertToByteArrayEquals(DateTime value, params byte[] expected)
            {
                CollectionAssert.AreEqual(expected, Types.DateTimeLong.ToByteArray(value));
            }
        }
    }
}
