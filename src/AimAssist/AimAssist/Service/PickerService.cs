using AimAssist.Core.Interfaces;
using AimAssist.UI.PickerWindows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Common.UI.Commands.Shortcus;

namespace AimAssist.Service
{
    /// <summary>
    /// ピッカーサービスの実装クラス
    /// </summary>
    public class PickerService : IPickerService
    {
        private static IntPtr beforeWindow;
        private static bool isActive;
        private readonly ICommandService commandService;
        private readonly IUnitsService unitsService;
        private readonly IEditorOptionService editorOptionService;
        private readonly IApplicationLogService logService;
        private TaskCompletionSource<string> tcs = new();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public PickerService(ICommandService commandService, IUnitsService unitsService,
            IEditorOptionService editorOptionService, IApplicationLogService logService)
        {
            this.commandService = commandService;
            this.unitsService = unitsService;
            this.editorOptionService = editorOptionService;
            this.logService = logService;
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

            tcs = new TaskCompletionSource<string>();

            try
            {
                isActive = true;

                // HotKey押下時のウィンドウのハンドルを取得
                beforeWindow = GetForegroundWindow();

                GetWindowThreadProcessId(beforeWindow, out var processId);
                Process process = Process.GetProcessById((int) processId);

                var window = new PickerWindow(process.ProcessName, commandService, unitsService, editorOptionService,
                    logService);
                window.Closed += (sender, _) =>
                {
                    if (sender is PickerWindow pickerWindow)
                    {
                        tcs.TrySetResult(pickerWindow.SnippetText);
                    }
                    else
                    {
                        tcs.TrySetResult(string.Empty);
                    }

                    isActive = false;
                    HandlePickerClosed(sender);
                };

                window.Show();
                window.Activate();

                return await tcs.Task;
            }
            catch (Exception ex)
            {
                isActive = false;
                tcs.TrySetException(ex);
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
                callback.Invoke(result);
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
            tcs.TrySetResult(string.Empty);
        }

        private static void HandlePickerClosed(object? sender)
        {
            string text = string.Empty;
            KeySequence? keySequence = null;

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