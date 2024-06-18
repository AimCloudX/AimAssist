using System.Windows.Input;

namespace AimAssist.UI
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(T parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(T parameter)
        {
            _execute(parameter);
        }

        public bool CanExecute(object? parameter)
        {
            if(parameter is T tparamter)
            {
                return _canExecute(tparamter);
            }

            return false;

        }

        public void Execute(object? parameter)
        {
            if(parameter is T tparamter)
            {
                 this._execute(tparamter);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(string commandName, Action execute)
        {
            CommandName = commandName;
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public string CommandName { get; }

        public void Execute()
        {
            _execute();
        }

        public void Execute(object? parameter)
        {
            _execute();
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
