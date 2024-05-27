using System.Windows;

namespace AimPicker.Combos
{
    public interface IPreviewFactory
    {
        UIElement Create(IUnitViewModel combo);

        bool IsKeepUiElement { get; }
    }

}
