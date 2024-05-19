using AimPicker.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClipboardAnalyzer
{
    public class ClipboardAnalyzerPreviewFactory : IPreviewFactory
    {
        public UIElement Create(ICombo combo)
        {
            return new ClipboardList();
        }
    }
}
