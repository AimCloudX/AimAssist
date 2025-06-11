using System.Windows;
using AimAssist.Core.Units;
using AimAssist.Units.Core.Units;
using Common.UI.WebUI;
using Common.UI.WebUI.Amazon;
using Common.UI.WebUI.LLM;

namespace AimAssist.Units.ViewProviders.Providers
{
    [ViewProvider(Priority = 100)]
    public class UrlViewProvider : IViewProvider
    {
        public int Priority => 100;

        public bool CanProvideView(Type unitType) => unitType == typeof(UrlUnit);

        public UIElement CreateView(IItem unit, IServiceProvider serviceProvider)
        {
            var urlUnit = (UrlUnit)unit;
            var url = urlUnit.Url;

            return url switch
            {
                var u when u.StartsWith("https://chatgpt") => new ChatGptControl(url),
                var u when u.StartsWith("https://claude.ai") => new ClaudeControl(url),
                var u when u.StartsWith("https://www.amazon") => new AmazonWebViewControl(url),
                _ => new WebViewControl(urlUnit.Url)
            };
        }
    }
}
