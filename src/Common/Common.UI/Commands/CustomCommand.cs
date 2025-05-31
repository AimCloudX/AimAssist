using System.Windows.Input;
using Common.UI.Commands.Shortcus;

namespace Common.UI.Commands
{
    public class CustomCommand : ICommand
    {
        public string Name { get; }
        public KeySequence KeySequence { get; set; }
        public Action ExecuteAction { get; }

        public CustomCommand(string name, KeySequence keySequence, Action executeAction)
        {
            Name = name;
            KeySequence = keySequence;
            ExecuteAction = executeAction;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            ExecuteAction?.Invoke();
        }
    }
}
