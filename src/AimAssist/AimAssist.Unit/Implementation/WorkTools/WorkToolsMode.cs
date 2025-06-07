using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using AimAssist.Core.Attributes;
using AimAssist.Units.Core.Modes;

namespace AimAssist.Units.Implementation.WorkTools
{
    [ModeDisplayOrder(10)]
    public class WorkToolsMode : ModeBase
    {
        private WorkToolsMode() : base(ModeName) { }

        public const string ModeName = "WorkTools";

        public static WorkToolsMode Instance { get; } = new WorkToolsMode();

        public override Control Icon => CreateIcon(PackIconKind.Toolbox);

        public override string Description => "登録されたWorkToolsを表示";
    }
}
