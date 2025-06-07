using System.Windows.Controls;
using Common.UI.Commands.Shortcus;

namespace AimAssist.Core.Units
{
    public interface IMode
    {
        Control Icon { get; }
        string Name { get; }
        string Description { get; }

        bool IsIncludeAllInclusive { get; }

        KeySequence? DefaultKeySequence { get; }
    }
}
