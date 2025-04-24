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
        [ImportMany(typeof(IUnitPlugin))] private IEnumerable<IUnitPlugin> _plugins;
        private readonly IApplicationLogService _applicationLogService;
        private readonly IEditorOptionService _editorOptionService;
        private bool _isPluginsLoaded = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="applicationLogService">アプリケーションログサービス</param>
        /// <param name="editorOptionService">エディターオプションサービス</param>
        public PluginsService(
            IApplicationLogService applicationLogService, 
            IEditorOptionService editorOptionService)
        {
            _applicationLogService = applicationLogService ?? throw new ArgumentNullException(nameof(applicationLogService));
            _editorOptionService = editorOptionService ?? throw new ArgumentNullException(nameof(editorOptionService));
            _plugins = new List<IUnitPlugin>();
        }

        /// <inheritdoc/>
        public void LoadCommandPlugins()
        {
            if (_isPluginsLoaded)
            {
                _applicationLogService.Log("プラグインは既にロードされています。");
                return;
            }

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
                    _applicationLogService.Log("プラグインディレクトリを作成します。");
                    
                    try
                    {
                        Directory.CreateDirectory(pluginPath);
                        _applicationLogService.Log($"プラグインディレクトリを作成しました: {pluginPath}");
                    }
                    catch (Exception ex)
                    {
                        _applicationLogService.Log($"プラグインディレクトリの作成に失敗しました: {ex.Message}");
                    }
                    
                    return;
                }
            }
            catch (Exception e)
            {
                _applicationLogService.Log($"プラグインのロード中にエラーが発生しました: {e.Message}");
                _applicationLogService.Log($"詳細: {e.StackTrace}");
                
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
                batch.AddExportedValue(_editorOptionService);
                container.Compose(batch);
                
                container.ComposeParts(this);
                _isPluginsLoaded = true;
                _applicationLogService.Log($"プラグインのロードが完了しました。プラグイン数: {_plugins.Count()}");
            }
            catch (CompositionException ce)
            {
                _applicationLogService.Log($"プラグインのコンポジション中にエラーが発生しました: {ce.Message}");
                _applicationLogService.Log($"詳細: {ce.StackTrace}");
                
                MessageBox.Show(
                    $"プラグインのコンポジション中にエラーが発生しました。詳細はログを確認してください。", 
                    "エラー", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                _applicationLogService.Log($"プラグインのロード中に予期しないエラーが発生しました: {ex.Message}");
                _applicationLogService.Log($"詳細: {ex.StackTrace}");
                
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
            if (!_isPluginsLoaded)
            {
                _applicationLogService.Log("プラグインがロードされていません。先にLoadCommandPluginsを呼び出してください。");
                return Enumerable.Empty<IUnitsFactory>();
            }
            
            var factories = new List<IUnitsFactory>();
            foreach (var plugin in _plugins)
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
                    _applicationLogService.Log($"プラグイン {plugin.GetType().Name} からファクトリの取得中にエラーが発生しました: {ex.Message}");
                    _applicationLogService.Log($"詳細: {ex.StackTrace}");
                }
            }

            return factories;
        }

        /// <inheritdoc/>
        public Dictionary<Type, Func<IUnit, UIElement>> GetConverters()
        {
            if (!_isPluginsLoaded)
            {
                _applicationLogService.Log("プラグインがロードされていません。先にLoadCommandPluginsを呼び出してください。");
                return new Dictionary<Type, Func<IUnit, UIElement>>();
            }
            
            var converters = new Dictionary<Type, Func<IUnit, UIElement>>();
            foreach (var plugin in _plugins)
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
                                _applicationLogService.Log($"コンバーター登録: {converter.Key.Name}");
                            }
                            else
                            {
                                _applicationLogService.Log($"警告: コンバーター {converter.Key.Name} は既に登録されています。スキップします。");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _applicationLogService.Log($"プラグイン {plugin.GetType().Name} からコンバーターの取得中にエラーが発生しました: {ex.Message}");
                    _applicationLogService.Log($"詳細: {ex.StackTrace}");
                }
            }

            return converters;
        }

        /// <summary>
        /// プラグインが読み込まれているかどうかを取得します
        /// </summary>
        public bool IsPluginsLoaded => _isPluginsLoaded;

        /// <summary>
        /// 読み込まれたプラグインの数を取得します
        /// </summary>
        public int PluginsCount => _plugins?.Count() ?? 0;
    }
}
