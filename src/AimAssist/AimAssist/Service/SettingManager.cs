using AimAssist.Core.Interfaces;
using Common;
using System.IO;
using System.Text.Json;
using Common.UI.Commands.Shortcus;

namespace AimAssist.Service
{
    public class SettingManager : ISettingManager
    {
        private string _settingsFilePath;

        public SettingManager()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolderPath = Path.Combine(appDataPath, "AimAssist");
            if (!Directory.Exists(appFolderPath))
            {
                Directory.CreateDirectory(appFolderPath);
            }

            _settingsFilePath = Path.Combine(appFolderPath, "KeyboardSettings.json");
        }

        // 設定を保存するメソッド
        public void SaveSettings(Dictionary<string, KeySequence> settings)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new KeySequenceConverter());
            options.WriteIndented = true;
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_settingsFilePath, json);
        }

        // 設定を読み込むメソッド
        public Dictionary<string, KeySequence> LoadSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                var options = new JsonSerializerOptions();
                options.Converters.Add(new KeySequenceConverter());
                options.WriteIndented = true;
                options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                string json = File.ReadAllText(_settingsFilePath);
                return JsonSerializer.Deserialize<Dictionary<string, KeySequence>>(json, options);
            }

            return new Dictionary<string, KeySequence>(); // デフォルト設定を返す
        }
    }
}
