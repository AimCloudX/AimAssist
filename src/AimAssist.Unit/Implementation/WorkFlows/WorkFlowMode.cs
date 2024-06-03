using AimAssist.Unit.Core.Mode;

namespace AimAssist.Unit.Implementation.WorkFlows
{
    public class WorkFlowMode : PikcerModeBase
    {
        private WorkFlowMode() : base(ModeName) { }

        public const string ModeName = "WorkFlow";

        public static WorkFlowMode Instance { get; } = new WorkFlowMode();

        public override string Prefix => ">";
        public override string Description => "登録されたWorkFlowを表示";
    }
}
