using AimPicker.Combos;
using AimPicker.Unit.Core.Mode;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimPicker.Unit.Core
{
    public interface IUnit
    {
        BitmapImage Icon { get; }

        IPickerMode Mode { get; }
        string Category { get; }

        string Name { get; }
        string Text { get; }

        UIElement GetUiElement();
    }
}
