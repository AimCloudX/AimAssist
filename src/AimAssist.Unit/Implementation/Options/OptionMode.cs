using AimAssist.Unit.Core.Mode;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AimAssist.Unit.Implementation.Options
{
    public class OptionMode : ModeBase
    {
        private OptionMode() : base(ModeName)
        {
        }
        public override Control Icon => CreateIcon(PackIconKind.Settings);

        public const string ModeName = "Options";

        public static OptionMode Instance { get; } = new OptionMode();

        public override bool IsApplyFiter => true;


        public override string Description => "AimAssistのオプション設定";
    }
}
