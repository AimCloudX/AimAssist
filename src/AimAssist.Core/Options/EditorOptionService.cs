using Common.UI.Editor;
using Newtonsoft.Json;
using System.IO;

namespace AimAssist.Core.Options
{
    public class EditorOptionService
    {
        public static EditorOption Option;
        private static FileSystemWatcher watcher;

        public static string OptionPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "editor.option.json");

        public static void LoadOption() 
        { 
            if(File.Exists(OptionPath))
            {
                var text = File.ReadAllText(OptionPath);
                var option = JsonConvert.DeserializeObject<EditorOption>(text);
                if(option == null)
                {
                    Option = new EditorOption();
                }
            }
            else
            {
                var option = new EditorOption();
                Option = option;
                SaveOption();
            }

            watcher = new FileSystemWatcher(Path.GetDirectoryName(EditorOptionService.OptionPath));
                // 監視する変更タイプを設定
                watcher.NotifyFilter = NotifyFilters.FileName
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.LastAccess
                                     | NotifyFilters.CreationTime;

                // 監視するファイルの種類を指定
                watcher.Filter = Path.GetFileName(EditorOptionService.OptionPath);

                // イベントハンドラを追加
                watcher.Changed += OnChanged;

                // 監視を開始
                watcher.EnableRaisingEvents = true;
        }

        private static void OnChanged(object source, FileSystemEventArgs e) =>
            EditorOptionService.LoadOption();

        public static void SaveOption()
        {
            var text = JsonConvert.SerializeObject(Option, Formatting.Indented);
            File.WriteAllText(OptionPath, text);
        }
    }
}
