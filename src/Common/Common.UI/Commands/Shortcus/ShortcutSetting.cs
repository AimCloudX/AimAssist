namespace Common.UI.Commands.Shortcus
{
    public class ShortcutSetting
    {
        public ShortcutSetting(string CommandName, KeySequence keySequence)
        {
            this.CommandName = CommandName;
            KeySequence = keySequence;
        }

        public string CommandName { get; set; }
        public KeySequence KeySequence { get; set; }
    }
}
