using AimAssist.Units.Core.Mode;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AimAssist.Units.Implementation.Caluculation
{
    public class CalcMode : ModeBase
    {
        private CalcMode() : base(ModeName) { }
        public override Control Icon => CreateIcon(PackIconKind.Wikipedia);

        public const string ModeName = "Calc";

        public static CalcMode Instance { get; } = new CalcMode();
        public override string Description => "計算";
    }
}
