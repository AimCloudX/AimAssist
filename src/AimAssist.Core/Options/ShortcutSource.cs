namespace AimAssist.Core.Options
{
    public class ShortcutSource
    {
        public ShortcutSource(string commandName, string gesture)
        {
            this.CommandName = commandName;
            this.Gesture = gesture;
            this.beforeGesutre = gesture;
        }

        public string CommandName { get; }

        public string Gesture { get; set; }

        public string beforeGesutre { get; }

        public bool IsModified
            => Gesture != beforeGesutre;
    }
}
