using System.Windows;
using System.Windows.Input;

namespace Common.UI.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<Window> _execute;

        public RelayCommand(string commandName, Action<Window> execute)
        {
            CommandName = commandName;
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public string CommandName { get; }

        public void Execute(Window window)
        {
            _execute(window);
        }

        public void Execute(object? parameter)
        {
            if(parameter is Window window)
            {

                _execute(window);
            }
            else
            {
                _execute(null);
            }
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

        public override bool Equals(object? obj)
        {
            if (obj is RelayCommand command)
            {
                return Equals(command);
            }

            return false;
        }

        public bool Equals(RelayCommand command)
        {
            return CommandName == command.CommandName;
        }
    }

    public class HotkeyCommand : RelayCommand
    {
        public HotkeyCommand(string commandName, Action<Window> execute) : base(commandName, execute)
        {
        }
    }
}
