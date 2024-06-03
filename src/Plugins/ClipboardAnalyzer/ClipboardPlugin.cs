using System.ComponentModel.Composition;
using AimAssist.Plugins;
using AimAssist.Unit.Core;


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
