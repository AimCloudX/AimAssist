namespace Common.UI.Commands.Shortcus
{
    public class ShortcutSetting
    {
        public ShortcutSetting(string commandName, KeySequence keySequence)
        {
            CommandName = commandName;
            KeySequence = keySequence;
        }

        public string CommandName { get; set; }
        public KeySequence KeySequence { get; set; }
    }
}
