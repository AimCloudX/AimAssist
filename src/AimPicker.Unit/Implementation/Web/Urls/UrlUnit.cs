using AimPicker.Combos;
using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using System.Windows;

namespace AimPicker.Unit.Implementation.Web.Urls
{
    public class UrlUnit : IUnit 
    {
        public string Name { get; }

        public UrlUnit(string name, string text, IPreviewFactory previewFactory)
        {
            Name = name;
            Text = text;
            this.PreviewFactory = previewFactory;
        }

        public string Text { get; }

        public IPreviewFactory PreviewFactory { get; }

        public IPickerMode Mode => UrlMode.Instance;

        public string Category => string.Empty;

        public UIElement GetUiElement()
        {
            return PreviewFactory.Create(this);
        }
    }
}
