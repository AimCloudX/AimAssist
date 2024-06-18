namespace AimAssist.Core.Options
{
    public class ShortcutSource
    {
        public ShortcutSource(string commandName, string gesture)
    {
        this.CommandName = commandName;
        this.Gesture = gesture;
    }

    public string CommandName { get; }

    public string Gesture { get; set; }
    }
}
