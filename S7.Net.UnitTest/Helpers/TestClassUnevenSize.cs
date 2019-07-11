using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S7.Net.UnitTest.Helpers
{
    public class TestClassUnevenSize
    {
        public bool Bool { get; set; }
        public byte[] Bytes { get; set; }
        public bool[] Bools { get; set; }

        public TestClassUnevenSize(int byteCount, int bitCount)
        {
            Bytes = new byte[byteCount];
            Bools = new bool[bitCount];
        }
    }
}
