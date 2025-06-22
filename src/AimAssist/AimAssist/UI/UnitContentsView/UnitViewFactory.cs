using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.Options;
using AimAssist.Units.Implementation.Snippets;
using System.Windows;
using System.Windows.Controls;
using AimAssist.Units.ViewProviders;
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

            // DataTemplateRegistryを初期化
            DataTemplateRegistry.Initialize();
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
            return element ?? new ContentPresenter();
        }

        private UIElement CreateInner(UnitViewModel unit)
        {
            // 1. ViewProviderを最優先でチェック
            var provider = viewProviders.FirstOrDefault(p => p.CanProvideView(unit.Content.GetType()));
            if (provider != null)
            {
                return provider.CreateView(unit.Content, serviceProvider);
            }

            // 2. 自動登録されたDataTemplateをチェック
            var autoTemplate = DataTemplateRegistry.CreateView(unit.Content, serviceProvider);
            if (autoTemplate != null)
            {
                return autoTemplate;
            }

            // 3. 従来のswitch文による手動対応
            switch (unit.Content)
            {
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

            // 4. レガシー辞書による対応
            if (UnitToUIElementDictionary.TryGetValue(unit.Content.GetType(), out var value))
            {
                return value.Invoke(unit.Content);
            }

            // 5. フォールバック: ContentPresenter
            var contentPresenter = new ContentPresenter
            {
                Content = unit.Content
            };

            return contentPresenter;
        }
    }
}
