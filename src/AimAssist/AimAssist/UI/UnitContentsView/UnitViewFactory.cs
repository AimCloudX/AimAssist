using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.Computer;
using AimAssist.Units.Implementation.Options;
using AimAssist.Units.Implementation.Pdf;
using AimAssist.Units.Implementation.Snippets;
using AimAssist.Units.Implementation.Speech;
using AimAssist.Units.Implementation.Web.MindMeister;
using AimAssist.Units.Implementation.Web.Rss;
using Common.UI;
using Common.UI.WebUI;
using System.Windows;
using AimAssist.Services.Editors;
using AimAssist.Units.Implementation.ClipboardAnalyzer;
using AimAssist.Units.Implementation.ClipboardAnalyzer.UI;
using Common.UI.Markdown;
using Common.UI.WebUI.Amazon;
using Common.UI.WebUI.LLM;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextBox = System.Windows.Controls.TextBox;

namespace AimAssist.UI.UnitContentsView
{
    public class UnitViewFactory
    {
        public static Dictionary<Type, Func<IUnit, UIElement>> UnitToUIElementDictionary = new();

        private static Dictionary<string, UIElement> cache = new();
        private readonly ICommandService commandService;
        private readonly IEditorOptionService editorOptionService;
        private readonly IServiceProvider serviceProvider;
        private readonly IEnumerable<IViewProvider> viewProviders;

        public UnitViewFactory(
            ICommandService commandService, 
            IEditorOptionService editorOptionService,
            IServiceProvider serviceProvider,
            IEnumerable<IViewProvider> viewProviders)
        {
            this.commandService = commandService;
            this.editorOptionService = editorOptionService;
            this.serviceProvider = serviceProvider;
            this.viewProviders = viewProviders.OrderByDescending(p => p.Priority);
        }

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
            if (element != null)
            {
                cache.Add(unit.Name, element);
            }
            return element;
        }

        private UIElement CreateInner(UnitViewModel unit)
        {
            var provider = viewProviders.FirstOrDefault(p => p.CanProvideView(unit.Content.GetType()));
            if (provider != null)
            {
                return provider.CreateView(unit.Content, serviceProvider);
            }

            switch (unit.Content)
            {
                case TranscriptionUnit:
                    return new SpeechControl();
                case ComputerUnit:
                    return new ComputerView();
                case PdfMergeUnit:
                    return new PdfMergerControl();
                case RssSettingUnit:
                    return new RssControl();
                case ClipboardUnit:
                    return new ClipboardList(editorOptionService);
                case ShortcutOptionUnit:
                    return new CustomizeKeyboardShortcutsSettings(commandService);
                case SnippetModelUnit model:
                    return new TextBox()
                    {
                        Text = model.Code,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(0)
                    };
                default:
                    break;
            }

            if (UnitToUIElementDictionary.TryGetValue(unit.Content.GetType(), out var value))
            {
                return value.Invoke(unit.Content);
            }

            var contentPresenter = new ContentPresenter
            {
                Content = unit.Content
            };

            return contentPresenter;
        }
    }
}
