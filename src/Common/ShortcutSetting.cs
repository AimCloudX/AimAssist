using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ShortcutSetting
    {
        public ShortcutSetting(string CommandName, KeySequence keySequence)
        {
            this.CommandName = CommandName;
            this.KeySequence = keySequence;
        }

        public string CommandName { get; set; }
        public KeySequence KeySequence { get; set; }
    }
}
