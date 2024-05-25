using AimPicker.Service.Plugins;
using System.ComponentModel.Composition;
using ClipboardAnalyzer.UI;
using AimPicker.DomainModels;


namespace ClipboardAnalyzer.Plugins
{
    [Export(typeof(IComboPlugin))]
    public class ClipboardComboPlugin : IComboPlugin
    {
        public IEnumerable<ICombo> GetCombo()
        {
            var text = System.Windows.Clipboard.ContainsText() ? System.Windows.Clipboard.GetText() : string.Empty;

            yield return new WorkFlowCombo("ClipboardAnalyzer", text, new ClipboardAnalyzerPreviewFactory());
        }
    }
}
