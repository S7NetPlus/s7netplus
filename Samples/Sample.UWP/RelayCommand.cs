using System;
using System.Windows.Input;

namespace Sample.UWP
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public RelayCommand(Action execute, Func<bool> canExecute) : this(_ => execute(), _ => canExecute())
        {
        }

        public RelayCommand(Action<object> execute) : this(execute, _ => true)
        {
        }

        public RelayCommand(Action execute) : this(_ => execute())
        {
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => canExecute.Invoke(parameter);

        public void Execute(object parameter) => execute(parameter);

        public void TriggerCanExecuteChange()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}