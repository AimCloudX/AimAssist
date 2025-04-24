using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Units.Core;
using AimAssist.Units.Core.Units;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows;

namespace AimAssist.Plugins
{
    /// <summary>
    /// プラグインサービスの実装
    /// </summary>
    public class PluginsService : IPluginsService
    {
        [ImportMany(typeof(IUnitplugin))] private IEnumerable<IUnitplugin> _plugins;
        private readonly IApplicationLogService _applicationLogService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="applicationLogService">アプリケーションログサービス</param>
        public PluginsService(IApplicationLogService applicationLogService)
        {
            _applicationLogService = applicationLogService;
            _plugins = new List<IUnitplugin>();
        }

        /// <inheritdoc/>
        public void LoadCommandPlugins()
        {
            // MEFコンテナを作成してプラグインをロード
            var catalog = new AggregateCatalog();
            var pluginPath = Path.Combine(Environment.CurrentDirectory, "Plugins");
            try
            {
                if (Directory.Exists(pluginPath))
                {
                    _applicationLogService.Log($"プラグインディレクトリを読み込みます: {pluginPath}");
                    catalog.Catalogs.Add(new DirectoryCatalog(pluginPath));
                }
                else
                {
                    _applicationLogService.Log($"プラグインディレクトリが見つかりません: {pluginPath}");
                    return;
                }
            }
            catch (Exception e)
            {
                _applicationLogService.Log($"プラグインのロード中にエラーが発生しました: {e.Message}");
                MessageBox.Show($"プラグインのロード中にエラーが発生しました: {e.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
            _applicationLogService.Log($"プラグインのロードが完了しました。プラグイン数: {_plugins.Count()}");
        }

        /// <inheritdoc/>
        public IEnumerable<IUnitsFacotry> GetFactories()
        {
            var factories = new List<IUnitsFacotry>();
            foreach (var plugin in _plugins)
            {
                try
                {
                    factories.AddRange(plugin.GetUnitsFactory());
                }
                catch (Exception ex)
                {
                    _applicationLogService.Log($"プラグインからファクトリの取得中にエラーが発生しました: {ex.Message}");
                }
            }

            return factories;
        }

        /// <inheritdoc/>
        public Dictionary<Type, Func<IUnit, UIElement>> GetConverters()
        {
            var converters = new Dictionary<Type, Func<IUnit, UIElement>>();
            foreach (var plugin in _plugins)
            {
                try
                {
                    foreach (var converter in plugin.GetUIElementConverters())
                    {
                        converters.TryAdd(converter.Key, converter.Value);
                    }
                }
                catch (Exception ex)
                {
                    _applicationLogService.Log($"プラグインからコンバーターの取得中にエラーが発生しました: {ex.Message}");
                }
            }

            return converters;
        }
    }
}
