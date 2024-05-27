using AimPicker.Unit.Core;

namespace AimPicker.Plugins
{
    public interface IComboPlugin
    {
        IEnumerable<IUnit> GetCombo();
    }
}
