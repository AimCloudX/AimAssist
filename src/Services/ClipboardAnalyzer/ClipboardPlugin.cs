using System.ComponentModel.Composition;
using System.Windows;
using AimAssist.Core.Units;
using AimAssist.Plugins;
using AimAssist.Units.Core;
using AimAssist.Units.Core.Units;


namespace ClipboardAnalyzer
{
    [Export(typeof(IUnitplugin))]
    public class ClipboardPlugin : IUnitplugin
    {
        public Dictionary<Type, Func<IUnit, UIElement>> GetUIElementConverters()
        {
            return new Dictionary<Type, Func<IUnit, UIElement>> {
                {typeof(ClipboardUnit), (unit)=>new ClipboardList() } };
        }

        public IEnumerable<IUnitsFacotry> GetUnitsFactory()
        {
            yield return new ClipboardUnitsFacotry();
        }
    }
}
