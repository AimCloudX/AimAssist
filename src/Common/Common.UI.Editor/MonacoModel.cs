using System.Runtime.InteropServices;

namespace Common.UI.Editor;

[ComVisible(true)]
public class MonacoModel
{
    public event Action<MonacoModel>? TextChanged;

    public event Action<MonacoModel>? RequestSave;

    private string text = string.Empty;

    public string Text
    {
        get => text;
        set
        {
            if (text != value)
            {
                text = value;
                TextChanged?.Invoke(this);
            }
        }
    }

    public string Language { get; set; } = string.Empty;

    public void OnRequestSave()
    {
        RequestSave?.Invoke(this);
    }
}

[ComVisible(true)]
public class KeyChange(string key, string command, string mode)
{
    public string Key { get; set; } = key;

    public string Command { get; set; } = command;
    public string Mode { get; set; } = mode;
}

public class KeyEventMessage(string type, string key, bool ctrlKey, bool shiftKey, bool altKey)
{
    public string Type { get; set; } = type;
    public string Key { get; set; } = key;
    public bool CtrlKey { get; set; } = ctrlKey;
    public bool ShiftKey { get; set; } = shiftKey;
    public bool AltKey { get; set; } = altKey;
}