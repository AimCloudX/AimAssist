using AimAssist.Core.Editors;
using AimAssist.UI.Combos;
using AimAssist.UI.Combos.Commands;
using AimAssist.UI.Options;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.ApplicationLog;
using AimAssist.Units.Implementation.Options;
using AimAssist.Units.Implementation.Snippets;
using AimAssist.Units.Implementation.Speech;
using AimAssist.Units.Implementation.Web.BookSearch;
using AimAssist.Units.Implementation.Web.Rss;
using AimAssist.Units.Implementation.WorkTools;
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
        
        public UIElement Create(UnitViewModel unit, bool createNew = false)
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

        private static UIElement CreateInner(UnitViewModel unit)
        {
            switch (unit.Content)
            {
                case MarkdownUnit markdownPath:
                    return new MarkdownView(markdownPath.FullPath);
                case TranscriptionUnit speechModel:
                    return new SpeechControl();
                case BookSearchSettingUnit:
                    return new BookSearchControl();
                case RssSettingUnit:
                    return new RssControl();
                case OptionUnit:
                    var optionEditor = new AimEditor();

                    optionEditor.NewTab(WorkItemOptionService.OptionPath);
                    optionEditor.NewTab(EditorOptionService.OptionPath);
                    if (File.Exists(EditorOptionService.Option.CustomVimKeybindingPath))
                    {
                        optionEditor.NewTab(EditorOptionService.Option.CustomVimKeybindingPath);
                    }

                    return optionEditor;
                case EditorUnit editorUnit:
                    var editor = new AimEditor();
                    editor.NewTab(editorUnit.FullPath);
                    return editor;
                case ShortcutOptionUnit:
                    return new CustomizeKeyboardShortcutsSettings();
                case SnippetUnit model:
                    return new System.Windows.Controls.TextBox()
                    {
                        Text = model.Code,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(0)
                    };
                case UrlUnit urlPath:
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
                case AppLogUnit:
                    return new ApplicationLogControl();

                default:
                    break;
            }

            if(UnitToUIElementDicotionary.TryGetValue(unit.Content.GetType(), out var value))
            {
                return value.Invoke(unit.Content);
            }

            return null;
        }
    }
}
