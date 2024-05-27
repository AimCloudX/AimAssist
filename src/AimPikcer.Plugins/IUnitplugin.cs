using AimPicker.Unit.Core;

namespace AimPicker.Plugins
{
    public interface IUnitplugin
    {
        IEnumerable<IUnit> GetUnits();
    }
}
