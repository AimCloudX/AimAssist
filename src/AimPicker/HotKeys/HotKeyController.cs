using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace AimPicker.HotKeys;

public class HotKeyController : IDisposable
{
    private nint _windowHandle;
    private Dictionary<int, HotKeyItem> _hotkeyList = new Dictionary<int, HotKeyItem>();

    private const int WM_HOTKEY = 0x0312;

    [DllImport("user32.dll")]
    private static extern int RegisterHotKey(nint hWnd, int id, int modKey, int vKey);

    [DllImport("user32.dll")]
    private static extern int UnregisterHotKey(nint hWnd, int id);

    public HotKeyController(Window window)
    {
        var host = new WindowInteropHelper(window);
        _windowHandle = host.Handle;

        ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
    }

    private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
    {
        if (msg.message != WM_HOTKEY) { return; }

        var id = msg.wParam.ToInt32();
        var hotkey = _hotkeyList[id];

        hotkey?.Handler
            ?.Invoke(this, EventArgs.Empty);
    }

    private int _hotkeyID = 0x0000;

    private const int MAX_HOTKEY_ID = 0xC000;

    /// <summary>
    /// 引数で指定された内容で、HotKeyを登録します。
    /// </summary>
    /// <param name="modKey"></param>
    /// <param name="key"></param>
    /// <param name="handler"></param>
    /// <returns></returns>
    public bool Register(ModifierKeys modKey, Key key, EventHandler handler)
    {
        var modKeyNum = (int)modKey;
        var vKey = KeyInterop.VirtualKeyFromKey(key);

        // HotKey登録
        while (_hotkeyID < MAX_HOTKEY_ID)
        {
            var ret = RegisterHotKey(_windowHandle, _hotkeyID, modKeyNum, vKey);

            if (ret != 0)
            {
                // HotKeyのリストに追加
                var hotkey = new HotKeyItem(modKey, key, handler);
                _hotkeyList.Add(_hotkeyID, hotkey);
                _hotkeyID++;
                return true;
            }
            _hotkeyID++;
        }

        return false;
    }

    /// <summary>
    /// 引数で指定されたidのHotKeyを登録解除します。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool Unregister(int id)
    {
        var ret = UnregisterHotKey(_windowHandle, id);
        return ret == 0;
    }

    /// <summary>
    /// 引数で指定されたmodKeyとkeyの組み合わせからなるHotKeyを登録解除します。
    /// </summary>
    /// <param name="modKey"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Unregister(ModifierKeys modKey, Key key)
    {
        var item = _hotkeyList
            .FirstOrDefault(o => o.Value.ModifierKeys == modKey && o.Value.Key == key);
        var isFound = !item.Equals(default(KeyValuePair<int, HotKeyItem>));

        if (isFound)
        {
            var ret = Unregister(item.Key);
            if (ret)
            {
                _hotkeyList.Remove(item.Key);
            }
            return ret;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 登録済みのすべてのHotKeyを解除します。
    /// </summary>
    /// <returns></returns>
    public bool UnregisterAll()
    {
        var result = true;
        foreach (var item in _hotkeyList)
        {
            result &= Unregister(item.Key);
        }

        return result;
    }

    #region IDisposable Support
    private bool disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // マネージリソースの破棄
            }

            // アンマネージリソースの破棄
            UnregisterAll();

            disposedValue = true;
        }
    }

    ~HotKeyController()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion

}