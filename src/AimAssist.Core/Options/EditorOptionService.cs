using Common.UI.Editor;
using Newtonsoft.Json;
using System.IO;

namespace AimAssist.Core.Options
{
    public static class EditorOptionService
    {
        public static EditorOption Option;

        public static string OptionPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "editor.option.json");


        public static void LoadOption() 
        { 
            if(File.Exists(OptionPath))
            {
                var text = File.ReadAllText(OptionPath);
                Option = JsonConvert.DeserializeObject<EditorOption>(text);
            }
            else
            {
                var option = new EditorOption();
                Option = option;
                SaveOption();
            }
        }

        public static void SaveOption()
        {
            var text = JsonConvert.SerializeObject(Option, Formatting.Indented);
            File.WriteAllText(OptionPath, text);
        }
    }
}
