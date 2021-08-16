using S7.Net.Types;

namespace S7.Net.UnitTest.Helpers
{
    public struct TestStruct
    {
        /// <summary>
        /// DB1.DBX0.0
        /// </summary>
        public bool BitVariable00;

        public bool BitVariable01;
        public bool BitVariable02;
        public bool BitVariable03;
        public bool BitVariable04;
        public bool BitVariable05;
        public bool BitVariable06;
        public bool BitVariable07;

        /// <summary>            
        /// DB1.DBX1.0           
        /// </summary>           
        public bool BitVariable10;

        public bool BitVariable11;
        public bool BitVariable12;
        public bool BitVariable13;
        public bool BitVariable14;
        public bool BitVariable15;
        public bool BitVariable16;
        public bool BitVariable17;

        /// <summary>           
        /// DB1.DBW2            
        /// </summary>          
        public short IntVariable;

        /// <summary>
        /// DB1.DBD4
        /// </summary>
        public double LRealVariable;

        /// <summary>
        /// DB1.DBD8
        /// </summary>
        public float RealVariable;

        /// <summary>
        /// DB1.DBD12
        /// </summary>
        public int DIntVariable;

        /// <summary>
        /// DB1.DBD16
        /// </summary>
        public ushort DWordVariable;

        /// <summary>
        /// DB1.DBX20.0
        /// </summary>
        [S7String(S7StringType.S7WString, 10)] 
        public string WStringVariable;

        /// <summary>
        /// DB1.DBX44.0
        /// </summary>
        [S7String(S7StringType.S7String, 10)]
        public string StringVariable;
    }
}
