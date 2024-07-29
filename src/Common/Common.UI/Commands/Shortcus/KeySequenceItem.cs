using Common.Commands.Shortcus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.UI.Commands.Shortcus
{
    public class KeySequenceItem
    {
        public string Title { get; set; }
        public string ApplicationName { get; }
        public string Operation { get; set; }
        public KeySequence Sequence { get; set; }

        public KeySequenceItem(KeySequence sequence, string operation, string title, string applicationName)
        {
            Sequence = sequence;
            Operation = operation;
            Title = title;
            ApplicationName = applicationName;
        }

        public string GetKeyString()
        {
            return Sequence.ToString();
        }
    }
}
