using AimAssist.Core.Interfaces;
using AimAssist.UI.PickerWindows;
using Common.Commands.Shortcus;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AimAssist.Service
{
    /// <summary>
    /// ピッカーサービスの実装クラス
    /// </summary>
    public class PickerService : IPickerService
    {
        private static IntPtr beforeWindow;
        private static bool isActive;
        private readonly ICommandService _commandService;
        private readonly IUnitsService _unitsService;
        private readonly IEditorOptionService _editorOptionService;
        private TaskCompletionSource<string> _tcs;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="commandService">コマンドサービス</param>
        /// <param name="unitsService">ユニットサービス</param>
        /// <param name="editorOptionService">エディタオプションサービス</param>
        public PickerService(ICommandService commandService, IUnitsService unitsService, IEditorOptionService editorOptionService)
        {
            _commandService = commandService;
            _unitsService = unitsService;
            _editorOptionService = editorOptionService;
        }

        /// <summary>
        /// ピッカーウィンドウを表示し、結果を返す
        /// </summary>
        /// <returns>選択された結果</returns>
        public async Task<string> ShowPicker()
        {
            if (isActive) 
            { 
                return string.Empty; 
            }

            _tcs = new TaskCompletionSource<string>();
            
            try
            {
                isActive = true;

                // HotKey押下時のウィンドウのハンドルを取得
                beforeWindow = GetForegroundWindow();

                GetWindowThreadProcessId(beforeWindow, out var processId);
                Process process = Process.GetProcessById((int)processId);

                var window = new PickerWindow(process.ProcessName, _commandService, _unitsService, _editorOptionService);
                window.Closed += (sender, e) => 
                {
                    if (sender is PickerWindow pickerWindow)
                    {
                        _tcs.TrySetResult(pickerWindow.SnippetText ?? string.Empty);
                    }
                    else
                    {
                        _tcs.TrySetResult(string.Empty);
                    }
                    
                    isActive = false;
                    HandlePickerClosed(sender, e);
                };
                
                window.Show();
                window.Activate();

                return await _tcs.Task;
            }
            catch (Exception ex)
            {
                isActive = false;
                _tcs.TrySetException(ex);
                throw;
            }
        }

        /// <summary>
        /// ピッカーウィンドウを表示し、選択後にコールバックを実行
        /// </summary>
        /// <param name="callback">選択後のコールバック</param>
        /// <returns>実行タスク</returns>
        public async Task ShowPickerWithCallback(Action<string> callback)
        {
            try
            {
                string result = await ShowPicker();
                callback?.Invoke(result);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"ピッカーウィンドウでエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// ピッカーウィンドウを閉じる
        /// </summary>
        public void ClosePicker()
        {
            isActive = false;
            _tcs?.TrySetResult(string.Empty);
        }

        private static void HandlePickerClosed(object? sender, EventArgs e)
        {
            string text = string.Empty;
            KeySequence keySequence = null;

            try
            {
                if (sender is PickerWindow window)
                {
                    text = window.SnippetText;
                    if (!string.IsNullOrEmpty(text))
                    {
                        System.Windows.Clipboard.SetText(text);
                    }
                    keySequence = window.KeySequence;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

            // 元のプロセスをアクティブにする
            SetForegroundWindow(beforeWindow);
            Thread.Sleep(100); // アクティブになるまで少し待つ

            // SendKeysを使用してキーを送信するためにSystem.Windows.Formsを追加する必要がある
            if (keySequence != null)
            {
                var keyText = keySequence.Parse();
                SendKeys.SendWait(keyText);
            }
            else if (!string.IsNullOrEmpty(text))
            {
                SendKeys.SendWait("^v");
            }
        }
    }
}
