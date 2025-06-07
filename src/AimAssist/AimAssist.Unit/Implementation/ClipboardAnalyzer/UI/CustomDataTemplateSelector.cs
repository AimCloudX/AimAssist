using System.Windows;
using System.Windows.Controls;
using AimAssist.Services.ClipboardAnalyzer.DomainModels;

namespace AimAssist.Units.Implementation.ClipboardAnalyzer.UI
{
    public class CustomDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? StringTemplate { get; set; }
        public DataTemplate? ImageTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ClipboardData data)
            {
                return StringTemplate ?? base.SelectTemplate(item, container);
            }

            if (item is ClipboardImage image)
            {
                return ImageTemplate ?? base.SelectTemplate(item, container);
            }

            return StringTemplate ?? base.SelectTemplate(item, container);
        }
    }
}
