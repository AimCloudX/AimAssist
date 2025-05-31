using Common.UI.Commands.Shortcus;

namespace AimAssist.Services.Options
{
    public class ShortcutSource
    {
        public ShortcutSource(string commandName, KeySequence gesture)
        {
            CommandName = commandName;
            Gesture = gesture;
            beforeGesutre = gesture;
        }

        public string CommandName { get; }

        public KeySequence Gesture { get; set; }

        public KeySequence beforeGesutre { get; }

        public bool IsModified
            => Gesture != beforeGesutre;
    }
}
