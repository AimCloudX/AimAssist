using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using AimAssist.Core.Interfaces;
using AimAssist.Service;
using Common.UI.Commands;

namespace AimAssist.HotKeys;

public class HotKeyController : IDisposable
{
    private readonly nint windowHandle;
    private readonly Dictionary<int, HotKeyItem> hotkeyList = new Dictionary<int, HotKeyItem>();
    private ICheatSheetController? cheatSheetController;

    private const int WmHotkey = 0x0312;

    [DllImport("user32.dll")]
    private static extern int RegisterHotKey(nint hWnd, int id, int modKey, int vKey);

    [DllImport("user32.dll")]
    private static extern int UnregisterHotKey(nint hWnd, int id);

    public HotKeyController(Window window, ICheatSheetController cheatSheetController)
    {
        this.window = window;
        this.cheatSheetController = cheatSheetController;
        var host = new WindowInteropHelper(window);
        windowHandle = host.Handle;
        //cheatSheetController = new CheatSheetController(window.Dispatcher);

        ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
    }

    private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
    {
        if (msg.message != WmHotkey) { return; }

        var id = msg.wParam.ToInt32();
        if (hotkeyList.TryGetValue(id, out var hotkey))
        {
            hotkey.Command?.Execute(null!);
        }
    }

    private int hotkeyId = 0x0000;

    private const int MaxHotkeyId = 0xC000;
    private readonly Window window;

    /// <summary>
    /// 引数で指定された内容で、HotKeyを登録します。
    /// </summary>
    /// <param name="modKey"></param>
    /// <param name="key"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    public bool Register(ModifierKeys modKey, Key key, RelayCommand? command = null!)
    {
        var modKeyNum = (int)modKey;
        var vKey = KeyInterop.VirtualKeyFromKey(key);

        // HotKey登録
        while (hotkeyId < MaxHotkeyId)
        {
            var ret = RegisterHotKey(windowHandle, hotkeyId, modKeyNum, vKey);

            if (ret != 0)
            {
                // HotKeyのリストに追加
                var hotkey = new HotKeyItem(modKey, key, command);
                hotkeyList.Add(hotkeyId, hotkey);
                hotkeyId++;
                return true;
            }
            hotkeyId++;
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
        var ret = UnregisterHotKey(windowHandle, id);
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
        var item = hotkeyList
            .FirstOrDefault(o => o.Value.ModifierKeys == modKey && o.Value.Key == key);
        var isFound = !item.Equals(default(KeyValuePair<int, HotKeyItem>));

        if (isFound)
        {
            var ret = Unregister(item.Key);
            if (ret)
            {
                hotkeyList.Remove(item.Key);
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
        foreach (var item in hotkeyList)
        {
            result &= Unregister(item.Key);
        }

        return result;
    }

    #region IDisposable Support
    private bool disposedValue;

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
        cheatSheetController?.Dispose();
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
