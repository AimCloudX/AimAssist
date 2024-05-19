using System.Windows;

namespace AimPicker.UI.Combos
{
    public interface IPreviewFactory
    {
        UIElement Create(IComboViewModel combo);
    }

}
