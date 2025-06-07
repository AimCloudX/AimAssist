namespace Common.UI.Editor
{
    public class EditorOption(string customVimKeybindingPath, EditorMode mode)
    {
        public EditorMode Mode { get; } = mode;
        public string CustomVimKeybindingPath { get; } = customVimKeybindingPath;

        public static EditorOption Default()
        {
            return new EditorOption(string.Empty, EditorMode.Standard);
        }
    }
}