using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimAssist.Unit.Implementation.Web.Rss
{
    public class RssUnit : IUnit
    {
        public BitmapImage Icon => new BitmapImage();

        public IMode Mode => RssMode.Instance;

        public string Category => string.Empty;

        public string Name => "Rss Settings";

        public string Text => "RssSettings";

        public UIElement GetUiElement()
        {
            return new RssControl();
        }
    }
}
