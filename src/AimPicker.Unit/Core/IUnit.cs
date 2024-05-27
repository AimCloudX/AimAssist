using AimPicker.Combos;
using System.Windows;

namespace AimPicker.Unit.Core
{
    public interface IUnit
    {
        string Name { get; }
        string Text { get; }
        IPreviewFactory PreviewFactory { get; }
    }
}
