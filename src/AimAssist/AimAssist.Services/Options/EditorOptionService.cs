using System.IO;
using AimAssist.Core.Interfaces;
using Common.UI.Editor;
using Newtonsoft.Json;

namespace AimAssist.Services.Options
{
    public class EditorOptionService : IEditorOptionService
    {
        private EditorOption _option;
        private FileSystemWatcher _watcher;

        /// <summary>
        /// エディター設定
        /// </summary>
        public EditorOption Option => _option;

        /// <summary>
        /// 設定ファイルのパス
        /// </summary>
        public string OptionPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "AimAssist", 
            "editor.option.json");

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EditorOptionService()
        {
            LoadOption();
        }

        /// <summary>
        /// 設定をロードする
        /// </summary>
        public void LoadOption()
        {
            if (File.Exists(OptionPath))
            {
                try {
                    var text = File.ReadAllText(OptionPath);
                    var option = JsonConvert.DeserializeObject<EditorOption>(text);
                    if (option == null)
                    {
                        option = new EditorOption();
                    }

                    _option = option;
                }
                catch(Exception _)
                {
                    _option = new EditorOption();
                }
            }
            else
            {
                var option = new EditorOption();
                _option = option;
                SaveOption();
            }

            SetupFileWatcher();
        }

        /// <summary>
        /// ファイル監視を設定する
        /// </summary>
        private void SetupFileWatcher()
        {
            // 既に監視している場合は解除
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= OnChanged;
                _watcher.Dispose();
            }

            _watcher = new FileSystemWatcher(Path.GetDirectoryName(OptionPath));
            // 監視する変更タイプを設定
            _watcher.NotifyFilter = NotifyFilters.FileName
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.CreationTime;

            // 監視するファイルの種類を指定
            _watcher.Filter = Path.GetFileName(OptionPath);

            // イベントハンドラを追加
            _watcher.Changed += OnChanged;

            // 監視を開始
            _watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// ファイル変更時のイベントハンドラ
        /// </summary>
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            LoadOption();
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        public void SaveOption()
        {
            var text = JsonConvert.SerializeObject(_option, Formatting.Indented);
            File.WriteAllText(OptionPath, text);
        }
    }
}
