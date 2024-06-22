using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Common
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
