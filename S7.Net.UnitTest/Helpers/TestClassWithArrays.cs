using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S7.UnitTest.Helpers
{
    public class TestClassWithArrays
    {
        public bool Bool { get; set; }
        public bool[] BoolValues { get; set; } = new bool[2];
        public bool[] Fillers { get; set; } = new bool[13];

        public short[] Shorts { get; set; } = new short[2];
        public ushort[] UShorts { get; set; } = new ushort[2];
        public int[] Ints { get; set; } = new int[2];
        public double[] Doubles { get; set; } = new double[2];
        public float[] Singles { get; set; } = new float[2];

        public short Short { get; set; }
        public ushort UShort { get; set; }
        public int Int { get; set; }
        public double Double { get; set; }
        public float Single { get; set; }
    }
}
