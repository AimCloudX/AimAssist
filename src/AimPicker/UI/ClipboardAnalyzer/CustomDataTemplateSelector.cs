using AimPicker.Service.Clipboard;
using System.Windows;
using System.Windows.Controls;

namespace AimPicker.UI.ClipboardAnalyzer
{
    internal class CustomDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StringTemplate { get; set; }
        public DataTemplate ImageTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if(item is ClipboardService.ClipboardData data)
            {
                return StringTemplate;
            }

            if(item is ClipboardService.ClipboardImage image)
            {
                return ImageTemplate;
            }

            return StringTemplate;

        }
    }
}
