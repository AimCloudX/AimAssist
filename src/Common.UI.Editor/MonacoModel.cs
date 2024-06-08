using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.UI.Editor
{
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

}
[ComVisible(true)]
public class keyChange(string key, string command, string mode)
{
    public string key { get; set; } = key;

    public string command { get; set; }
    public string mode { get; set; } = mode;
}
