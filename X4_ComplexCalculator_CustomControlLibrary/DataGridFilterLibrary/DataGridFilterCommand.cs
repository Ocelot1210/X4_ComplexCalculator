using System;
using System.Windows.Input;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary
{
    public class DataGridFilterCommand : ICommand
    {
        private readonly Action<object> _Action;

        public DataGridFilterCommand(Action<object> action)
        {
            _Action = action;
        }

        public void Execute(object parameter)
        {
            _Action?.Invoke(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
