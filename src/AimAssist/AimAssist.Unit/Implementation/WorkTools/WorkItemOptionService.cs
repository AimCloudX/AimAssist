using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Units.Implementation.WorkTools
{
    public class WorkItemOptionService
    {
        public static ConfigModel Option;
        private static FileSystemWatcher watcher;

        public static string OptionPath => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AimAssist", "workitem.option.json");

        public static void LoadOption()
        {
            if (File.Exists(OptionPath))
            {
                try {

                    var text = File.ReadAllText(OptionPath);
                    var option = JsonConvert.DeserializeObject<ConfigModel>(text);
                    if (option == null)
                    {
                        option = new ConfigModel();
                    }

                    Option = option;
                }
                catch(Exception _)
                {
                    Option = new ConfigModel();
                }
            }
            else
            {
                var option = new ConfigModel();
                Option = option;
                SaveOption();
            }

            watcher = new FileSystemWatcher(Path.GetDirectoryName(OptionPath));
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

        private static void OnChanged(object source, FileSystemEventArgs e) =>
            LoadOption();

        public static void SaveOption()
        {
            var text = JsonConvert.SerializeObject(Option, Formatting.Indented);
            File.WriteAllText(OptionPath, text);
        }
    }
    }
