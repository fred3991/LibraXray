using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LibraXray.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
            => (_execute, _canExecute) = (execute, canExecute);

        public bool CanExecute(object parameter)
            => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter)
            => _execute(parameter);
    }
}
