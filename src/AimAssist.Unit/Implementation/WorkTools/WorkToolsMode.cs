using AimAssist.Unit.Core.Mode;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace AimAssist.Unit.Implementation.WorkTools
{
    public class WorkToolsMode : ModeBase
    {
        private WorkToolsMode() : base(ModeName) { }

        public const string ModeName = "WorkTools";

        public static WorkToolsMode Instance { get; } = new WorkToolsMode();

        public override Control Icon => CreateIcon(PackIconKind.Toolbox);

        public override string Description => "登録されたWorkToolsを表示";
    }
}
