using AimAssist.Unit.Core;

namespace AimAssist.Plugins
{
    public interface IUnitplugin
    {
        IEnumerable<IUnitsFacotry> GetUnitsFactory();
    }
}
