using AimPicker.Combos;
using AimPicker.Unit.Core.Mode;
using System.Windows;

namespace AimPicker.Unit.Core
{
    public interface IUnit
    {
        IPickerMode Mode { get; }
        string Category { get; }

        string Name { get; }
        string Text { get; }

        UIElement GetUiElement();
    }
}
