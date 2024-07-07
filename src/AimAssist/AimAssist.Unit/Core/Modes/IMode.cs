using Common.Commands.Shortcus;
using System.Windows.Controls;

namespace AimAssist.Units.Core.Mode
{
    public interface IMode
    {
        Control Icon { get; }
        string Name { get; }
        string Description { get; }

        KeySequence DefaultKeySequence { get; }
    }
}
