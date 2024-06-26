using AimAssist.UI.Combos.Commands;
using Common.UI;
using Common.UI.ChatGPT;
using System.Windows;

namespace AimAssist.Unit.Implementation.Web
{
    public class WebViewPreviewFactory
    {
        public bool IsKeepUiElement => true;

        public UIElement Create(string url)
        {
            if (url.StartsWith("https://chatgpt"))
            {
                return new ChatGptControl(url);
            }
            if (url.StartsWith("https://claude.ai/"))
            {
                return new ClaudeControl(url);
            }

            if (url.StartsWith("https://www.amazon"))
            {
                return new AmazonWebViewControl(url);
            }

            return new WebViewControl(url);
        }
    }
}
