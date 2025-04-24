using System.ComponentModel.Composition;
using System.Windows;
using AimAssist;
using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Plugins;
using AimAssist.Units.Core;
using AimAssist.Units.Core.Units;
using Microsoft.Extensions.DependencyInjection;


namespace ClipboardAnalyzer
{
    [Export(typeof(IUnitPlugin))]
    public class ClipboardPlugin : IUnitPlugin
    {
        private readonly IEditorOptionService _editorOptionService;

        [ImportingConstructor]
        public ClipboardPlugin(IEditorOptionService editorOptionService = null)
        {
            // MEFによるインポートでIEditorOptionServiceがnullの場合にのみサービスロケーターを使用
            _editorOptionService = editorOptionService ?? ((App)App.Current)?._serviceProvider?.GetRequiredService<IEditorOptionService>();
        }

        public Dictionary<Type, Func<IUnit, UIElement>> GetUIElementConverters()
        {
            return new Dictionary<Type, Func<IUnit, UIElement>> {
                {typeof(ClipboardUnit), (unit)=> {
                    return new ClipboardList(_editorOptionService);
                }}
            };
        }

        public IEnumerable<IUnitsFactory> GetUnitsFactory()
        {
            yield return new ClipboardUnitsFactory();
        }
    }
}
