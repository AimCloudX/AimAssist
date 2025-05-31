using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using AimAssist.Units.Core.Modes;


namespace AimAssist.Units.Implementation.Web.MindMeister
{
    public class MindMeisterMode : ModeBase
    {
        private MindMeisterMode() : base(ModeName) { }

        public override Control Icon => CreateIcon(PackIconKind.ThoughtBubble);
        public const string ModeName = "MindMeister";

        public static MindMeisterMode Instance { get; } = new MindMeisterMode();

        public override string Description => "MindMeister";
    }
}
