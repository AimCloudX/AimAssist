using System.Windows;

namespace AimPicker.UI.Combos
{
    public interface IComboViewModel
    {
        string Name { get; }

        string Text { get; }

        UIElement Create();
    }

}
