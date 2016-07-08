namespace S7.Net.Types
{
    public class DataItem
    {
        public DataType DataType { get; set; }
        public VarType VarType { get; set; }
        public int DB { get; set; }
        public int StartByteAdr { get; set; }
        public int Count { get; set; }

        public object Value { get; set; }

        public DataItem()
        {
            Count = 1;
        }
    }
}
