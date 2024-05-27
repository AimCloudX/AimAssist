using AimPicker.Unit.Core;
using System.Windows;

namespace AimPicker.Combos
{
    public interface IPreviewFactory
    {
        UIElement Create(IUnit combo);
    }

}
