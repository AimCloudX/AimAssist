using System;
using System.Windows.Input;
using Common.UI.Commands;

namespace AimAssist.HotKeys;

public class HotKeyItem
{
    public ModifierKeys ModifierKeys { get; private set; }
    public Key Key { get; private set; }
    public RelayCommand Command { get; private set; }

    public HotKeyItem(ModifierKeys modKey, Key key, RelayCommand handler)
    {
        ModifierKeys = modKey;
        Key = key;
        Command = handler;
    }
}