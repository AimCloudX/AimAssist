using System.ComponentModel.Composition;
using ClipboardAnalyzer.UI;
using AimPicker.Plugins;
using AimPicker.Combos.Mode.WorkFlows;
using AimPicker.Unit.Core;


namespace ClipboardAnalyzer.Plugins
{
    [Export(typeof(IUnitplugin))]
    public class ClipboardPlugin : IUnitplugin
    {
        public IEnumerable<IUnit> GetUnits()
        {
            var text = System.Windows.Clipboard.ContainsText() ? System.Windows.Clipboard.GetText() : string.Empty;

            yield return new WorkFlowCombo("ClipboardAnalyzer", text, new ClipboardAnalyzerPreviewFactory());
        }
    }
}
