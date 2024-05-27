using AimPicker.Combos;
using AimPicker.UI.Combos;
using AimPicker.Unit.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AimPicker.Plugins
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

        public IEnumerable<IUnit> GetCombos()
        {
            var combos = new List<IUnit>();
            foreach (var plugin in _plugins)
            {
                combos.AddRange(plugin.GetUnits());
            }

            return combos;
        }
    }
}
