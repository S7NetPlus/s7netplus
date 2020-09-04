using System.Collections.Generic;

namespace S7.Net.Types
{
    class ByteArray
    {
        List<byte> list = new List<byte>();

        public byte this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }

        public byte[] Array
        {
            get { return list.ToArray(); }
        }

        public int Length => list.Count;

        public ByteArray()
        {
            list = new List<byte>();
        }

        public ByteArray(int size)
        {
            list = new List<byte>(size);
        }
        
        public void Clear()
        {
            list = new List<byte>();
        }

        public void Add(byte item)
        {
            list.Add(item);
        }

        public void AddWord(ushort value)
        {
            list.Add((byte) (value >> 8));
            list.Add((byte) value);
        }

        public void Add(byte[] items)
        {
            list.AddRange(items);
        }

        public void Add(IEnumerable<byte> items)
        {
            list.AddRange(items);
        }

        public void Add(ByteArray byteArray)
        {
            list.AddRange(byteArray.Array);
        }
    }
}
