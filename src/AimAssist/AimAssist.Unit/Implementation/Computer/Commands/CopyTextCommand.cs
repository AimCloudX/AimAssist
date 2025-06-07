using System.Windows.Input;

namespace AimAssist.Units.Implementation.Computer.Commands
{
    public class CopyTextCommand : ICommand
    {
        private readonly Action<object?> execute;
        private readonly Predicate<object?>? canExecute;

        public CopyTextCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            this.execute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
