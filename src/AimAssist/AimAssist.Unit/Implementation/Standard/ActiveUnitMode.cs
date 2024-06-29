using AimAssist.Units.Core.Mode;
using Common.Commands.Shortcus;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using System.Windows.Input;

namespace AimAssist.Units.Implementation.Standard
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
