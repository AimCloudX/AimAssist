using AimAssist.UI;
using Common;
using System.Windows.Controls;

namespace AimAssist.Unit.Core.Mode
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
