using System.Runtime.InteropServices;

namespace Common.UI.Editor;

[ComVisible(true)]
public class MonacoModel
{
    public event Action<MonacoModel>? TextChanged;

    public event Action<MonacoModel>? RequestSave;

    private string _Text = string.Empty;
    public string Text
    {
        get => _Text;
        set
        {
            if (_Text != value)
            {
                _Text = value;
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
public class keyChange(string key, string command, string mode)
{
    public string key { get; set; } = key;

    public string command { get; set; }
    public string mode { get; set; } = mode;
}

public class KeyEventMessage
{
    public string type { get; set; }
    public string key { get; set; }
    public bool ctrlKey { get; set; }
    public bool shiftKey { get; set; }
    public bool altKey { get; set; }
}