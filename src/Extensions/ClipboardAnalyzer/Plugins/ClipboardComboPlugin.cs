﻿using AimPicker.Domain;
using AimPicker.Service.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using ClipboardAnalyzer.UI;
using AimPicker.UI.Combos;
using AimPicker.UI.Combos.Commands;


namespace ClipboardAnalyzer.Plugins
{
    [Export(typeof(IComboPlugin))]
    public class ClipboardComboPlugin : IComboPlugin
    {
        public IEnumerable<IComboViewModel> GetCombo()
        {
            yield return new PickerCommandViewModel("ClipboardAnalyzer", System.Windows.Clipboard.GetText(), new ClipboardAnalyzerPreviewFactory());
        }
    }
}