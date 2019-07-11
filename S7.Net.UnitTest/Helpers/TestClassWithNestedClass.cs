using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S7.Net.UnitTest.Helpers
{
    class TestClassInnerWithBool
    {
        public bool BitVariable00 { get; set; }
    }

    class TestClassInnerWithByte
    {
        public byte ByteVariable00 { get; set; }
    }

    class TestClassInnerWithShort
    {
        public short ShortVarialbe00 { get; set; }
    }

    class TestClassWithNestedClass
    {
        /// <summary>
        /// DB1.DBX0.0
        /// </summary>
        public bool BitVariable00 { get; set; }

        /// <summary>
        /// DB1.DBX0.1
        /// </summary>
        public TestClassInnerWithBool BitVariable01 { get; set; } = new TestClassInnerWithBool();

        /// <summary>
        /// DB1.DBB1.0
        /// </summary>
        public TestClassInnerWithByte ByteVariable02 { get; set; } = new TestClassInnerWithByte();

        /// <summary>
        /// DB1.DBX2.0
        /// </summary>
        public bool BitVariable03 { get; set; }

        /// <summary>
        /// DB1.DBW4
        /// </summary>
        public TestClassInnerWithShort ShortVariable04 { get; set; } = new TestClassInnerWithShort();
    }
}
