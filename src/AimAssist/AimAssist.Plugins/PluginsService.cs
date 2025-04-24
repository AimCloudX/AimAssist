using AimAssist.Core.Units;
using AimAssist.Units.Core;
using AimAssist.Units.Core.Units;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows;

namespace AimAssist.Plugins
{
    public class PluginsService
    {
        [ImportMany(typeof(IUnitplugin))] private IEnumerable<IUnitplugin> _plugins;


        public void LoadCommandPlugins()
        {
            // MEFコンテナを作成してプラグインをロード
            var catalog = new AggregateCatalog();
            //catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            var pluginPath = Path.Combine(Environment.CurrentDirectory, "Plugins");
            try
            {
                if (Directory.Exists(pluginPath))
                {
                    catalog.Catalogs.Add(new DirectoryCatalog(pluginPath));
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            // catalog.Catalogs.Add(new DirectoryCatalog(Environment.CurrentDirectory));
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        public IEnumerable<IUnitsFacotry> GetFactories()
        {
            var combos = new List<IUnitsFacotry>();
            foreach (var plugin in _plugins)
            {
                combos.AddRange(plugin.GetUnitsFactory());
            }

            return combos;
        }

        public Dictionary<Type, Func<IUnit, UIElement>> GetConterters()
        {
            var dic = new Dictionary<Type, Func<IUnit, UIElement>>();
            foreach (var plugin in _plugins)
            {
                foreach(var converters in plugin.GetUIElementConverters())
                {
                    dic.TryAdd(converters.Key, converters.Value);
                }
            }

            return dic;
        }
    }
}
