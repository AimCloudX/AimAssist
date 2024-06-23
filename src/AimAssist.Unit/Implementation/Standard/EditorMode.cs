using AimAssist.Unit.Core.Mode;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace AimAssist.Unit.Implementation.Standard
{
    public class EditorMode : PikcerModeBase
    {
        public EditorMode() : base(ModeName)
        {
        }
        public override Control Icon => CreateIcon(PackIconKind.Star);

        public const string ModeName = "Edit";

        public static EditorMode Instance { get; } = new EditorMode();

        public override string Description => "Editor";
    }
}
