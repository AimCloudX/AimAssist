using System.IO;
using AimAssist.Core.Interfaces;
using Common.UI.Editor;
using Newtonsoft.Json;

namespace AimAssist.Services.Options
{
    public class EditorOptionService : IEditorOptionService
    {
        private FileSystemWatcher? watcher;

        /// <summary>
        /// エディター設定
        /// </summary>
        public EditorOption Option { get; private set; } = EditorOption.Default();

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
                        option = EditorOption.Default();
                    }

                    this.Option = option;
                }
                catch(Exception)
                {
                    Option = EditorOption.Default();
                }
            }
            else
            {
                var option = EditorOption.Default();
                this.Option = option;
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
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Changed -= OnChanged;
                watcher.Dispose();
            }

            watcher = new FileSystemWatcher(Path.GetDirectoryName(OptionPath) ?? string.Empty);
            // 監視する変更タイプを設定
            watcher.NotifyFilter = NotifyFilters.FileName
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.CreationTime;

            // 監視するファイルの種類を指定
            watcher.Filter = Path.GetFileName(OptionPath);

            // イベントハンドラを追加
            watcher.Changed += OnChanged;

            // 監視を開始
            watcher.EnableRaisingEvents = true;
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
            var text = JsonConvert.SerializeObject(Option, Formatting.Indented);
            File.WriteAllText(OptionPath, text);
        }
    }
}
