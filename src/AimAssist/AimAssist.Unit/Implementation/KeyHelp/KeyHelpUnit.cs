using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;
using Common.Commands.Shortcus;
using Common.UI.Commands.Shortcus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Units.Implementation.KeyHelp
{
    public class KeyHelpUnit : IUnit
    {
        public KeyHelpUnit(KeySequenceItem keyItem) {
            KeyItem = keyItem;
        }

        public IMode Mode => KeyHelpMode.Instance;

        public string Name => KeyItem.Operation;

        public string Description => KeyItem.GetKeyString();

        public string Category => KeyItem.ApplicationName;
        public KeySequenceItem KeyItem { get; }
    }
}
