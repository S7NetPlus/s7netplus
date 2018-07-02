using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using JetBrains.Annotations;
using S7.Net;

namespace Sample.UWP
{
    public class VariableViewModel : INotifyPropertyChanged
    {
        private readonly PlcViewModel plcViewModel;
        private object value;
        private byte bitAdr;
        private VarType varType;
        private int startByte;
        private int db;
        private DataType dataType;

        public VariableViewModel(PlcViewModel plcViewModel)
        {
            this.plcViewModel = plcViewModel;

            Remove = new RelayCommand(() => plcViewModel.Variables.Remove(this));
            Read = new RelayCommand(ReadImpl);
            Write = new RelayCommand(WriteImpl);
        }

        public List<VarType> VarTypes => EnumHelper.GetValues<VarType>();
        public List<DataType> DataTypes => EnumHelper.GetValues<DataType>();

        public ICommand Remove { get; }
        public ICommand Read { get; }
        public ICommand Write { get; }

        public object Value
        {
            get => value;
            set
            {
                if (Equals(value, this.value)) return;
                this.value = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ReadImpl()
        {
            try
            {
                Value = plcViewModel.Plc.Read(DataType, Db, StartByte, VarType, 1, BitAdr);
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }

        private void WriteImpl()
        {
            object nativeValue;
            var localBitAdr = -1;
            switch (VarType)
            {
                case VarType.Bit:
                    localBitAdr = BitAdr;
                    return;
                case VarType.Int:
                    nativeValue = short.Parse((string) Value);
                    break;
                case VarType.DInt:
                    nativeValue = int.Parse((string) Value);
                    break;
                default:
                    return;
            }

            plcViewModel.Plc.Write(DataType, Db, StartByte, nativeValue, localBitAdr);
        }

        public byte BitAdr
        {
            get => bitAdr;
            set
            {
                if (value == bitAdr) return;
                bitAdr = value;
                OnPropertyChanged();
            }
        }

        public VarType VarType
        {
            get => varType;
            set
            {
                if (value == varType) return;
                varType = value;
                OnPropertyChanged();
            }
        }

        public int StartByte
        {
            get => startByte;
            set
            {
                if (value == startByte) return;
                startByte = value;
                OnPropertyChanged();
            }
        }

        public int Db
        {
            get => db;
            set
            {
                if (value == db) return;
                db = value;
                OnPropertyChanged();
            }
        }

        public DataType DataType
        {
            get => dataType;
            set
            {
                if (value == dataType) return;
                dataType = value;
                OnPropertyChanged();
            }
        }
    }
}