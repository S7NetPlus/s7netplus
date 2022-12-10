
using S7.Net.Types;

namespace S7.Net.UnitTest.Helpers
{
    class TestClass
    {
        /// <summary>
        /// DB1.DBX0.0
        /// </summary>
        public bool BitVariable00 { get; set; }
        public bool BitVariable01 { get; set; }
        public bool BitVariable02 { get; set; }
        public bool BitVariable03 { get; set; }
        public bool BitVariable04 { get; set; }
        public bool BitVariable05 { get; set; }
        public bool BitVariable06 { get; set; }
        public bool BitVariable07 { get; set; }

        /// <summary>
        /// DB1.DBX1.0
        /// </summary>
        public bool BitVariable10 { get; set; }
        public bool BitVariable11 { get; set; }
        public bool BitVariable12 { get; set; }
        public bool BitVariable13 { get; set; }
        public bool BitVariable14 { get; set; }
        public bool BitVariable15 { get; set; }
        public bool BitVariable16 { get; set; }
        public bool BitVariable17 { get; set; }

        /// <summary>
        /// DB1.DBW2
        /// </summary>
        public short IntVariable { get; set; }

        /// <summary>
        /// DB1.DBD4
        /// </summary>
        public double LRealVariable { get; set; }

        /// <summary>
        /// DB1.DBD8
        /// </summary>
        public float RealVariable { get; set; }

        /// <summary>
        /// DB1.DBD12
        /// </summary>
        public int DIntVariable { get; set; }

        /// <summary>
        /// DB1.DBD16
        /// </summary>
        public ushort DWordVariable { get; set; }

        /// <summary>
        /// DB1.DBX20.0
        /// </summary>
        [S7String(S7StringType.S7WString, 10)]
        public string WStringVariable { get; set; }
        /// <summary>
        /// DB1.DBX44.0
        /// </summary>
        [S7String(S7StringType.S7String, 10)]
        public string StringVariable { get; set; }
    }
}
