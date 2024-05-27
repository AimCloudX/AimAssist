using System.Windows;

namespace AimPicker.Combos
{
    public interface IUnitViewModel
    {
        string Name { get; }

        string Text { get; }

        UIElement Create();
    }

}
