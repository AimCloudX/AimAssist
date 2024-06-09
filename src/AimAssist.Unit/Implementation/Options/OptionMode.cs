using AimAssist.Unit.Core.Mode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Unit.Implementation.Options
{
    public class OptionMode : PikcerModeBase
    {
        private OptionMode() : base(ModeName)
        {
        }

        public const string ModeName = "Options";

        public static OptionMode Instance { get; } = new OptionMode();

        public override string Prefix => "op ";

        public override string Description => "AimAssistのオプション設定";
    }
}
