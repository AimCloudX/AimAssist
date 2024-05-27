using System.ComponentModel.Composition;
using ClipboardAnalyzer.UI;
using AimPicker.Plugins;
using AimPicker.Combos.Mode.WorkFlows;
using AimPicker.Unit.Core;


namespace ClipboardAnalyzer.Plugins
{
    [Export(typeof(IComboPlugin))]
    public class ClipboardComboPlugin : IComboPlugin
    {
        public IEnumerable<IUnit> GetCombo()
        {
            var text = System.Windows.Clipboard.ContainsText() ? System.Windows.Clipboard.GetText() : string.Empty;

            yield return new WorkFlowCombo("ClipboardAnalyzer", text, new ClipboardAnalyzerPreviewFactory());
        }
    }
}
