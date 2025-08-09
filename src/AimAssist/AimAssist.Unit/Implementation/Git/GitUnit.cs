using AimAssist.Core.Attributes;
using AimAssist.Core.Units;
using AimAssist.Units.Core.Modes;
using AimAssist.Units.Implementation.WorkTools;

namespace AimAssist.Units.Implementation.Git
{
    [AutoRegisterUnit("Git", Priority = 90, IsEnabled = true)]
    public class GitUnit : IUnit
    {
        public IMode Mode => WorkToolsMode.Instance;
        public string Name => "Git";
        public string Description => "Gitリポジトリの管理とブランチ操作";
        public string Category => "Git";
    }
}