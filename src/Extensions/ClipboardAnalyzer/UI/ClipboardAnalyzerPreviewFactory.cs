using AimPicker.Domain;
using System.Windows;

namespace ClipboardAnalyzer.UI
{
    public class ClipboardAnalyzerPreviewFactory : IPreviewFactory
    {
        public UIElement Create(ICombo combo)
        {
            return new ClipboardList();
        }
    }
}
