using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class AppSettings
    {
        public List<ShortcutSetting> Shortcuts { get; set; } = new List<ShortcutSetting>();

        public void SaveSettings()
        {
            // JSON形式で設定を保存
            string json = JsonConvert.SerializeObject(this);
            File.WriteAllText("settings.json", json);
        }

        public static AppSettings LoadSettings()
        {
            if (File.Exists("settings.json"))
            {
                string json = File.ReadAllText("settings.json");
                return JsonConvert.DeserializeObject<AppSettings>(json);
            }
            return new AppSettings();
        }
    }
}
