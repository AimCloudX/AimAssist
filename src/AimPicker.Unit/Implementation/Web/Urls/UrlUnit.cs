using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimPicker.Unit.Implementation.Web.Urls
{
    public class UrlUnit : IUnit 
    {
        public string Name { get; }

        public UrlUnit(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public BitmapImage Icon => new BitmapImage();
        public string Text => this.Path;
        public string Path { get; }

        public WebViewPreviewFactory PreviewFactory { get; } = new WebViewPreviewFactory();

        public IPickerMode Mode => UrlMode.Instance;

        public string Category => string.Empty;

        public UIElement GetUiElement()
        {
            return PreviewFactory.Create(this.Path);
        }
    }
}
