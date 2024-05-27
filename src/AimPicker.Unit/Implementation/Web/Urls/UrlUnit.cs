using AimPicker.Combos;
using AimPicker.Unit.Core;
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

        private UIElement uiElement;

        public UIElement Create()
        {
            if (uiElement == null)
            {
                uiElement = PreviewFactory.Create(this);
            }

            return uiElement;
        }
    }
}
