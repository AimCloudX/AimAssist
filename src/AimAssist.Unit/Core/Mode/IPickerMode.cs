using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AimAssist.Unit.Core.Mode
{
    public interface IPickerMode
    {
        Control Icon { get; }
        string Name { get; }
        string Description { get; }

        bool IsApplyFiter { get; }
    }
}
