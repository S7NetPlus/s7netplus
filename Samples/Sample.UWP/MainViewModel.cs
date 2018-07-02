using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Popups;
using JetBrains.Annotations;
using S7.Net;

namespace Sample.UWP
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Connect = new RelayCommand(ConnectImpl, () => !string.IsNullOrWhiteSpace(IpAddress));

            PropertyChanged += (s, e) => Connect.TriggerCanExecuteChange();
        }

        private CpuType cpuType;
        private string ipAddress;
        private short rack;
        private short slot;
        private PlcViewModel plcViewModel;

        public List<CpuType> CpuTypes => EnumHelper.GetValues<CpuType>();

        public CpuType CpuType
        {
            get => cpuType;
            set
            {
                if (value == cpuType) return;
                cpuType = value;
                OnPropertyChanged();
            }
        }

        public string IpAddress
        {
            get => ipAddress;
            set
            {
                if (value == ipAddress) return;
                ipAddress = value;
                OnPropertyChanged();
            }
        }

        public short Rack
        {
            get => rack;
            set
            {
                if (value == rack) return;
                rack = value;
                OnPropertyChanged();
            }
        }

        public short Slot
        {
            get => slot;
            set
            {
                if (value == slot) return;
                slot = value;
                OnPropertyChanged();
            }
        }

        public PlcViewModel PlcViewModel
        {
            get => plcViewModel;
            private set
            {
                if (Equals(value, plcViewModel)) return;
                plcViewModel = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand Connect { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ConnectImpl()
        {
            var plc = new Plc(CpuType, IpAddress, Rack, Slot);
            plc.Open();
            PlcViewModel = new PlcViewModel(plc);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
