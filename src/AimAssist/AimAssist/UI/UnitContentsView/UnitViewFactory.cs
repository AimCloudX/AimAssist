using AimAssist.Combos.Mode.Wiki;
using AimAssist.Core.Editors;
using AimAssist.UI.Combos.Commands;
using AimAssist.UI.Options;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.Options;
using AimAssist.Units.Implementation.Snippets;
using AimAssist.Units.Implementation.Speech;
using AimAssist.Units.Implementation.Web.BookSearch;
using AimAssist.Units.Implementation.Web.Rss;
using Common.UI;
using Common.UI.ChatGPT;
using Library.Options;
using System.IO;
using System.Windows;

namespace AimAssist.UI.UnitContentsView
{
    internal class UnitViewFactory
    {
        public static Dictionary<Type, Func<IUnit, UIElement>> UnitToUIElementDicotionary = new();

        private static Dictionary<string, UIElement> cash = new();
        
        public UIElement Create(IUnit unit, bool createNew = false)
        {
            if (createNew)
            {
                return CreateInner(unit);
            }

            if (cash.TryGetValue(unit.Name, out var uiElement))
            {
                return uiElement;
            }

            var element = CreateInner(unit);
            cash.Add(unit.Name, element);
            return element;
        }

        private static UIElement CreateInner(IUnit unit)
        {
            switch (unit.Content)
            {
                case MarkdownPath markdownPath:
                    return new MarkdownPreviewFactory().Create(markdownPath.FullPath);
                case SpeechModel speechModel:
                    return new SpeechControl();
                case BookSearchSetting:
                    return new BookSearchControl();
                case RssSetting:
                    return new RssControl();
                case OptionContent:
                    var editor = new AimEditor();
                    editor.NewTab(EditorOptionService.OptionPath);
                    if (File.Exists(EditorOptionService.Option.CustomVimKeybindingPath))
                    {
                        editor.NewTab(EditorOptionService.Option.CustomVimKeybindingPath);
                    }

                    return editor;
                case ShortcutOptionContent:
                    return new CustomizeKeyboardShortcutsSettings();
                case SnippetModel model:
                    return new System.Windows.Controls.TextBox()
                    {
                        Text = model.Code,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(0)
                    };
                case UrlPath urlPath:
                    var url = urlPath.Url;
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

                default:
                    break;
            }

            if(UnitToUIElementDicotionary.TryGetValue(unit.Content.GetType(), out var value))
            {
                return value.Invoke(unit);
            }

            return null;
        }
    }
}
