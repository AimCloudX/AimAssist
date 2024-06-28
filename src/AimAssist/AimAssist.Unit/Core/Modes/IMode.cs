using Common.Commands;
using Common.Commands.Shortcus;
using System.Windows.Controls;

namespace AimAssist.Units.Core.Mode
{
    public interface IMode
    {
        Control Icon { get; }
        string Name { get; }
        string Description { get; }

        bool IsApplyFiter { get; }

        void SetModeChangeCommandAction(Action action);

        KeySequence DefaultKeySequence { get; }

        RelayCommand ModeChangeCommand { get; }
    }
}
