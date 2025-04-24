using AimAssist.Core.Units;
using AimAssist.Units.Core;
using AimAssist.Units.Core.Units;
using System.Windows;

namespace AimAssist.Plugins
{
    public interface IUnitplugin
    {
        IEnumerable<IUnitsFacotry> GetUnitsFactory();

        Dictionary<Type, Func<IUnit, UIElement>> GetUIElementConverters();
    }
}
