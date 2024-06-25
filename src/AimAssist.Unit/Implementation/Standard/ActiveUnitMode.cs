using AimAssist.Unit.Core.Mode;
using Common;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using System.Windows.Input;

namespace AimAssist.Unit.Implementation.Standard
{
    public class ActiveUnitMode : ModeBase
    {
        public ActiveUnitMode() : base(ModeName)
        {
        }

        public override Control Icon => CreateIcon(PackIconKind.Star);

        public const string ModeName = "Active";

        public static ActiveUnitMode Instance { get; } = new ActiveUnitMode();

        public override string Description => "Active Units";

        public override KeySequence DefaultKeySequence =>  new KeySequence(Key.K, ModifierKeys.Control, Key.K, ModifierKeys.Control);
    }
}
