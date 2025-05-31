using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
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
        [ImportMany(typeof(IUnitPlugin))] private IEnumerable<IUnitPlugin> plugins;
        private readonly IEditorOptionService editorOptionService;
        private bool isPluginsLoaded;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="editorOptionService">エディターオプションサービス</param>
        public PluginsService(
            IEditorOptionService editorOptionService)
        {
            this.editorOptionService = editorOptionService ?? throw new ArgumentNullException(nameof(editorOptionService));
            plugins = new List<IUnitPlugin>();
        }

        /// <inheritdoc/>
        public void LoadCommandPlugins()
        {
            if (isPluginsLoaded)
            {
                return;
            }

            // MEFコンテナを作成してプラグインをロード
            var catalog = new AggregateCatalog();
            var pluginPath = Path.Combine(Environment.CurrentDirectory, "Plugins");
            try
            {
                if (Directory.Exists(pluginPath))
                {
                    catalog.Catalogs.Add(new DirectoryCatalog(pluginPath));
                }
                else
                {
                    
                    try
                    {
                        Directory.CreateDirectory(pluginPath);
                    }
                    catch (Exception)
                    {
                    }
                    
                    return;
                }
            }
            catch (Exception e)
            {
                
                MessageBox.Show(
                    $"プラグインのロード中にエラーが発生しました: {e.Message}", 
                    "エラー", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return;
            }

            try
            {
                var container = new CompositionContainer(catalog);
                
                // MEFコンテナに_editorOptionServiceを登録
                var batch = new CompositionBatch();
                batch.AddExportedValue(editorOptionService);
                container.Compose(batch);
                
                container.ComposeParts(this);
                isPluginsLoaded = true;
            }
            catch (CompositionException)
            {
                MessageBox.Show(
                    $"プラグインのコンポジション中にエラーが発生しました。詳細はログを確認してください。", 
                    "エラー", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    $"プラグインのロード中に予期しないエラーが発生しました。詳細はログを確認してください。", 
                    "エラー", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IUnitsFactory> GetFactories()
        {
            if (!isPluginsLoaded)
            {
                return Enumerable.Empty<IUnitsFactory>();
            }
            
            var factories = new List<IUnitsFactory>();
            foreach (var plugin in plugins)
            {
                try
                {
                    var pluginFactories = plugin.GetUnitsFactory();
                    if (pluginFactories != null)
                    {
                        factories.AddRange(pluginFactories);
                    }
                }
                catch (Exception ex)
                {
                }
            }

            return factories;
        }

        /// <inheritdoc/>
        public Dictionary<Type, Func<IUnit, UIElement>> GetConverters()
        {
            if (!isPluginsLoaded)
            {
                return new Dictionary<Type, Func<IUnit, UIElement>>();
            }
            
            var converters = new Dictionary<Type, Func<IUnit, UIElement>>();
            foreach (var plugin in plugins)
            {
                try
                {
                    var pluginConverters = plugin.GetUIElementConverters();
                    if (pluginConverters != null)
                    {
                        foreach (var converter in pluginConverters)
                        {
                            if (!converters.ContainsKey(converter.Key))
                            {
                                converters.Add(converter.Key, converter.Value);
                            }
                            else
                            {
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

            return converters;
        }

        /// <summary>
        /// プラグインが読み込まれているかどうかを取得します
        /// </summary>
        public bool IsPluginsLoaded => isPluginsLoaded;

        /// <summary>
        /// 読み込まれたプラグインの数を取得します
        /// </summary>
        public int PluginsCount => plugins?.Count() ?? 0;
    }
}
