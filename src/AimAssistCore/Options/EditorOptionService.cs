using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Options
{
    public static class EditorOptionService
    {
        public static EditorOption Option;

        public static string OptionPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "editor.option.json");


        public static void LoadOption() 
        { 



        }
    }

    public class EditorOption
    {
        public EditorMode Mode { get; set; }
        public string CustomVimKeybindingPath { get; set; }
    }

    public enum EditorMode
    {
        Standard,
        Vim,
    }

}
