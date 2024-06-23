using Common;

namespace AimAssist.Core.Options
{
    public class ShortcutSource
    {
        public ShortcutSource(string commandName, KeySequence gesture)
        {
            this.CommandName = commandName;
            this.Gesture = gesture;
            this.beforeGesutre = gesture;
        }

        public string CommandName { get; }

        public KeySequence Gesture { get; set; }

        public KeySequence beforeGesutre { get; }

        public bool IsModified
            => Gesture != beforeGesutre;
    }
}
