using System;
using System.Windows.Input;

namespace AimPicker.Service.HotKeys;

    public class HotKeyItem
    {
        public ModifierKeys ModifierKeys { get; private set; }
        public Key Key { get; private set; }
        public EventHandler Handler { get; private set; }

        public HotKeyItem(ModifierKeys modKey, Key key, EventHandler handler)
        {
            this.ModifierKeys = modKey;
            this.Key = key;
            this.Handler = handler;
        }
    }