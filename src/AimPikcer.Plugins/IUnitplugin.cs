using AimPicker.Unit.Core;

namespace AimPicker.Plugins
{
    public interface IUnitplugin
    {
        IEnumerable<IUnitsFacotry> GetUnitsFactory();
    }
}
