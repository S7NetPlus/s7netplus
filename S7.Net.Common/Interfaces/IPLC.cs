using System;

namespace S7.Net.Interfaces
{
    public interface IPlc : IDisposable
    {
        string IP { get; set; }
        bool IsConnected { get; }
        CpuType CPU { get; set; }
        Int16 Rack { get; set; }
        Int16 Slot { get; set; }
        string Name { get; set; }
        object Tag { get; set; }
        bool IsAvailable { get; }
        ErrorCode Open();
        void Close();
        byte[] ReadBytes(DataType dataType, int DB, int startByteAdr, int count);
        object Read(DataType dataType, int db, int startByteAdr, VarType varType, int varCount);
        object Read(string variable);
        object ReadStruct(Type structType, int db);
        void ReadClass(object sourceClass, int db);
        ErrorCode WriteBytes(DataType dataType, int db, int startByteAdr, byte[] value);
        object Write(DataType dataType, int db, int startByteAdr, object value);
        object Write(string variable, object value);
        ErrorCode WriteStruct(object structValue, int db);
        ErrorCode WriteClass(object classValue, int db);
		string LastErrorString { get; }
		ErrorCode LastErrorCode { get; }
    }
}