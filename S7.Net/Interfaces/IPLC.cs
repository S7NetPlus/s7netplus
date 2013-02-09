using System;

namespace S7
{
    public interface IPlc : IDisposable
    {
        string IP { get; set; }
        bool IsConnected { get; }
        CPU_Type CPU { get; set; }
        Int16 Rack { get; set; }
        Int16 Slot { get; set; }
        string Name { get; set; }
        object Tag { get; set; }
        bool IsAvailable { get; }
        ErrorCode Open();
        void Close();
        byte[] ReadBytes(DataType DataType, int DB, int StartByteAdr, int count);
        object Read(DataType DataType, int DB, int StartByteAdr, VarType VarType, int VarCount);
        object Read(string variable);
        object ReadStruct(Type structType, int DB);
        ErrorCode WriteBytes(DataType DataType, int DB, int StartByteAdr, byte[] value);
        object Write(DataType DataType, int DB, int StartByteAdr, object value);
        object Write(string variable, object value);
        ErrorCode WriteStruct(object structValue, int DB);
    }
}