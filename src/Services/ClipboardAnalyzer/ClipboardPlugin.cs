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
    [Export(typeof(IUnitplugin))]
    public class ClipboardPlugin : IUnitplugin
    {
        public Dictionary<Type, Func<IUnit, UIElement>> GetUIElementConverters()
        {
            return new Dictionary<Type, Func<IUnit, UIElement>> {
                {typeof(ClipboardUnit), (unit)=> {
                    var editorOptionService = ((App)App.Current)._serviceProvider.GetRequiredService<IEditorOptionService>();
                    return new ClipboardList(editorOptionService);
                }}
            };
        }

        public IEnumerable<IUnitsFacotry> GetUnitsFactory()
        {
            yield return new ClipboardUnitsFacotry();
        }
    }
}
