using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace S7.Net.UnitTest.TypeTests
{
    public static class TimeSpanTests
    {
        private static readonly TimeSpan SampleTimeSpan = new TimeSpan(12, 0, 59, 37, 856);

        private static readonly byte[] SampleByteArray = { 0x3E, 0x02, 0xE8, 0x00 };

        private static readonly byte[] SpecMinByteArray = { 0x80, 0x00, 0x00, 0x00 };
        
        private static readonly byte[] SpecMaxByteArray = { 0x7F, 0xFF, 0xFF, 0xFF };

        [TestClass]
        public class FromByteArray
        {
            [TestMethod]
            public void Sample()
            {
                AssertFromByteArrayEquals(SampleTimeSpan, SampleByteArray);
            }

            [TestMethod]
            public void SpecMinimum()
            {
                AssertFromByteArrayEquals(Types.TimeSpan.SpecMinimumTimeSpan, SpecMinByteArray);
            }

            [TestMethod]
            public void SpecMaximum()
            {
                AssertFromByteArrayEquals(Types.TimeSpan.SpecMaximumTimeSpan, SpecMaxByteArray);
            }

            private static void AssertFromByteArrayEquals(TimeSpan expected, params byte[] bytes)
            {
                Assert.AreEqual(expected, Types.TimeSpan.FromByteArray(bytes));
            }
        }

        [TestClass]
        public class ToByteArray
        {
            [TestMethod]
            public void Sample()
            {
                AssertToByteArrayEquals(SampleTimeSpan, SampleByteArray);
            }

            [TestMethod]
            public void SpecMinimum()
            {
                AssertToByteArrayEquals(Types.TimeSpan.SpecMinimumTimeSpan, SpecMinByteArray);
            }

            [TestMethod]
            public void SpecMaximum()
            {
                AssertToByteArrayEquals(Types.TimeSpan.SpecMaximumTimeSpan, SpecMaxByteArray);
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnTimeBeforeSpecMinimum()
            {
                Types.TimeSpan.ToByteArray(TimeSpan.FromDays(-25));
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsOnTimeAfterSpecMaximum()
            {
                Types.TimeSpan.ToByteArray(new TimeSpan(30, 15, 15, 15, 15));
            }

            private static void AssertToByteArrayEquals(TimeSpan value, params byte[] expected)
            {
                CollectionAssert.AreEqual(expected, Types.TimeSpan.ToByteArray(value));
            }
        }
    }
}
