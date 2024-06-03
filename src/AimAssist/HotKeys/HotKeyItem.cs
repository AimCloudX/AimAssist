using System;
using System.Windows.Input;

namespace AimAssist.HotKeys;

public class HotKeyItem
{
    public ModifierKeys ModifierKeys { get; private set; }
    public Key Key { get; private set; }
    public EventHandler Handler { get; private set; }

    public HotKeyItem(ModifierKeys modKey, Key key, EventHandler handler)
    {
        ModifierKeys = modKey;
        Key = key;
        Handler = handler;
    }
}