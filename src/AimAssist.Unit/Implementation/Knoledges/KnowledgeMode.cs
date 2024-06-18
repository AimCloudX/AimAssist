using AimAssist.Combos.Mode.Wiki;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Knoledges;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimAssist.Unit.Implementation.Knowledge
{
    public class KnowledgeMode : PikcerModeBase
    {
        private KnowledgeMode() : base(ModeName) { }

        public const string ModeName = "Knowledge";

        public static KnowledgeMode Instance { get; } = new KnowledgeMode();

        public override string Prefix => "kn ";
        public override string Description => "AimAssist開発のナレッジを表示";
    }

}
