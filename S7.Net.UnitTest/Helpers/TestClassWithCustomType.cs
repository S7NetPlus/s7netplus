using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S7.UnitTest.Helpers
{
    public class CustomType
    {
        public bool[] Bools { get; set; } = new bool[2];
        public bool[] Fillers { get; set; } = new bool[14];
    }

    public class TestClassWithCustomType
    {
        public int Int { get; set; }
        public CustomType CustomType { get; set; }
        public CustomType[] CustomTypes { get; set; } = new CustomType[2];
    }
}
