﻿using AimAssist.Core.Interfaces;
using Newtonsoft.Json;
using System.IO;
using AimAssist.Core.Model;

namespace AimAssist.Units.Implementation.Snippets
{
    public class SnippetOptionService : ISnippetOptionService
    {
        public ConfigModel Option { get; private set; } = ConfigModel.Default();
        private static FileSystemWatcher? watcher;

        public string OptionPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AimAssist", "snippet.option.json");

        public void LoadOption()
        {
            if (File.Exists(OptionPath))
            {
                try
                {

                    var text = File.ReadAllText(OptionPath);
                    var option = JsonConvert.DeserializeObject<ConfigModel>(text);
                    if (option == null)
                    {
                        option = ConfigModel.Default();
                    }

                    Option = option;
                }
                catch (Exception ex)
                {
                    // エラーログを出力
                    Console.WriteLine($"スニペットオプションの読み込みに失敗しました: {ex.Message}");
                    // デフォルト設定を適用
                    Option = ConfigModel.Default();
                }
            }
            else
            {
                var option = ConfigModel.Default();
                Option = option;
                SaveOption();
            }

            watcher = new FileSystemWatcher(Path.GetDirectoryName(OptionPath) ?? throw new InvalidOperationException("OptionPath directory is null"));
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

        private void OnChanged(object source, FileSystemEventArgs e) =>
            LoadOption();

        /// <summary>
        /// オプションをファイルに保存します
        /// </summary>
        /// <returns>保存が成功したかどうか</returns>
        public bool SaveOption()
        {
            try
            {
                var text = JsonConvert.SerializeObject(Option, Formatting.Indented);
                
                // ディレクトリが存在しない場合は作成
                var directory = Path.GetDirectoryName(OptionPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(OptionPath, text);
                return true;
            }
            catch (Exception ex)
            {
                // エラーログを出力
                Console.WriteLine($"スニペットオプションの保存に失敗しました: {ex.Message}");
                return false;
            }
        }
    }
}
