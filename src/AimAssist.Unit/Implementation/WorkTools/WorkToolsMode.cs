using AimAssist.Unit.Core.Mode;

namespace AimAssist.Unit.Implementation.WorkTools
{
    public class WorkToolsMode : PikcerModeBase
    {
        private WorkToolsMode() : base(ModeName) { }

        public const string ModeName = "WorkTools";

        public static WorkToolsMode Instance { get; } = new WorkToolsMode();

        public override string Prefix => ">";
        public override string Description => "登録されたWorkToolsを表示";
    }
}
