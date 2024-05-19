using AimPicker.UI.Combos;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimPicker.Service.Plugins
{
    public class PluginsService
    {
        [ImportMany(typeof(IComboPlugin))] private IEnumerable<IComboPlugin> _plugins;


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

        public IEnumerable<IComboViewModel> GetCombos()
        {
            var combos = new List<IComboViewModel>();
            foreach (var plugin in _plugins)
            {
                combos.AddRange(plugin.GetCombo());
            }

            return combos;
        }
    }
}
