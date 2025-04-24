using AimAssist.Core.Editors;
using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Core.Units.Units;
using AimAssist.UI.Combos;
using AimAssist.UI.Combos.Commands;
using AimAssist.UI.Options;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.ApplicationLog;
using AimAssist.Units.Implementation.CodeGenarator;
using AimAssist.Units.Implementation.Computer;
using AimAssist.Units.Implementation.Options;
using AimAssist.Units.Implementation.Pdf;
using AimAssist.Units.Implementation.Snippets;
using AimAssist.Units.Implementation.Speech;
using AimAssist.Units.Implementation.Web.MindMeister;
using AimAssist.Units.Implementation.Web.Rss;
using AimAssist.Units.Implementation.WorkTools;
using CodeGenerator;
using Common.UI;
using Common.UI.ChatGPT;
using Common.UI.WebUI;
using Library.Options;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;

namespace AimAssist.UI.UnitContentsView
{
    /// <summary>
    /// ユニットからUIを作成するファクトリークラス
    /// </summary>
    public class UnitViewFactory
    {
        public static Dictionary<Type, Func<IUnit, UIElement>> UnitToUIElementDictionary = new();

        private static Dictionary<string, UIElement> cache = new();
        private readonly ICommandService _commandService;
        private readonly IEditorOptionService _editorOptionService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="commandService">コマンドサービス</param>
        /// <param name="editorOptionService">エディタオプションサービス</param>
        public UnitViewFactory(ICommandService commandService, IEditorOptionService editorOptionService)
        {
            _commandService = commandService;
            _editorOptionService = editorOptionService;
        }
        
        /// <summary>
        /// ユニットからUIを作成します
        /// </summary>
        /// <param name="unit">ユニット</param>
        /// <param name="createNew">新規作成するかどうか</param>
        /// <returns>作成されたUI要素</returns>
        public UIElement Create(UnitViewModel unit, bool createNew = false)
        {
            if (createNew)
            {
                return CreateInner(unit);
            }

            if (cache.TryGetValue(unit.Name, out var uiElement))
            {
                return uiElement;
            }

            var element = CreateInner(unit);
            cache.Add(unit.Name, element);
            return element;
        }

        private UIElement CreateInner(UnitViewModel unit)
        {
            switch (unit.Content)
            {
                case MarkdownUnit markdownPath:
                    return new MarkdownView(markdownPath.FullPath);
                case TranscriptionUnit speechModel:
                    return new SpeechControl();
                case RssSettingUnit:
                    return new RssControl();
                case OptionUnit option:
                    var optionEditor = new AimEditor(_editorOptionService);
                    foreach(var filePath in option.OptionFilePaths)
                    {
                        optionEditor.NewTab(filePath);
                    }

                    return optionEditor;
                case EditorUnit editorUnit:
                    var editor = new AimEditor(_editorOptionService);
                    editor.NewTab(editorUnit.FullPath);
                    return editor;
                case ShortcutOptionUnit:
                    return new CustomizeKeyboardShortcutsSettings(_commandService);
                case SnippetUnit model:
                    return new System.Windows.Controls.TextBox()
                    {
                        Text = model.Code,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(0)
                    };
                case SnippetModelUnit model:
                    return new System.Windows.Controls.TextBox()
                    {
                        Text = model.Code,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(0)
                    };
                case MindMeisterUnit model:
                    return new MindMeisterViewControl(model);
                case MindMeisterItemUnit model:
                    return new WebViewControl(model.SearchUrl, model.Name);
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
                case PdfMergeUnit:
                    return new PdfMergerControl();
                case AppLogUnit:
                    return new ApplicationLogControl();
                case CodeGeneratorUnit:
                    return new CodeGeneratorControl();
                case ComputerUnit:
                    return new ComputerView();
                default:
                    break;
            }

            if(UnitToUIElementDictionary.TryGetValue(unit.Content.GetType(), out var value))
            {
                return value.Invoke(unit.Content);
            }

            return null;
        }
    }
}
