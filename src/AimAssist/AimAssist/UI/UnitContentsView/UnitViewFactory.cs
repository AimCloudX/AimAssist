﻿using AimAssist.Core.Editors;
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
                case RssSettingUnit:
                    return new RssControl();
                case OptionUnit option:
                    var optionEditor = new AimEditor();
                    foreach(var filePath in option.OptionFilePaths)
                    {
                        optionEditor.NewTab(filePath);
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

            if(UnitToUIElementDicotionary.TryGetValue(unit.Content.GetType(), out var value))
            {
                return value.Invoke(unit.Content);
            }

            return null;
        }
    }
}
