using AimPicker.Combos;
using System.Windows;

namespace ClipboardAnalyzer.UI
{
    public class ClipboardAnalyzerPreviewFactory : IPreviewFactory
    {
        public bool IsKeepUiElement => false;

        public UIElement Create(IUnitViewModel combo)
        {
            return new ClipboardList();
        }
    }
}
