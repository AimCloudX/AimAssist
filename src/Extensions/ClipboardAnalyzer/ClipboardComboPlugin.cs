using AimPicker.Domain;
using AimPicker.Service.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Composition;
using System.ComponentModel.Composition;


namespace ClipboardAnalyzer
{
    [Export(typeof(IComboPlugin))]
    public class ClipboardComboPlugin : IComboPlugin
    {
        public IEnumerable<ICombo> GetCombo()
        {
            yield return new PickerCommand("ClipboardAnalyzer", System.Windows.Clipboard.GetText(), new ClipboardAnalyzerPreviewFactory());
        }
    }
}
