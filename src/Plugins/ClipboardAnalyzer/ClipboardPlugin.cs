using System.ComponentModel.Composition;
using AimPicker.Plugins;
using AimPicker.Unit.Core;


namespace ClipboardAnalyzer
{
    [Export(typeof(IUnitplugin))]
    public class ClipboardPlugin : IUnitplugin
    {
        public IEnumerable<IUnitsFacotry> GetUnitsFactory()
        {
            yield return new ClipboardUnitsFacotry();
        }
    }
}
