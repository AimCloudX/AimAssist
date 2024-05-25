using AimPicker.UI.Combos;
using System.Windows;

namespace ClipboardAnalyzer.UI
{
    public class ClipboardAnalyzerPreviewFactory : IPreviewFactory
    {
        public bool IsKeepUiElement => false;

        public UIElement Create(IComboViewModel combo)
        {
            return new ClipboardList();
        }
    }
}
