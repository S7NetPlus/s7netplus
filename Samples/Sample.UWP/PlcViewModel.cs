using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using JetBrains.Annotations;
using S7.Net;

namespace Sample.UWP
{
    public class PlcViewModel : INotifyPropertyChanged
    {
        public PlcViewModel(Plc plc)
        {
            Plc = plc;

            AddVariable = new RelayCommand(() => Variables.Add(new VariableViewModel(this)));
        }

        public Plc Plc { get; }
        public ICommand AddVariable { get; }

        public ObservableCollection<VariableViewModel> Variables { get; } =
            new ObservableCollection<VariableViewModel>();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}