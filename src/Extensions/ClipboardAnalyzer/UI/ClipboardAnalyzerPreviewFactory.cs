using AimPicker.Combos;
using AimPicker.Unit.Core;
using System.Windows;

namespace ClipboardAnalyzer.UI
{
    public class ClipboardAnalyzerPreviewFactory : IPreviewFactory
    {
        public bool IsKeepUiElement => false;

        public UIElement Create(IUnit combo)
        {
            return new ClipboardList();
        }
    }
}
