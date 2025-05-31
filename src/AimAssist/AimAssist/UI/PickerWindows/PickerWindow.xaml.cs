using AimAssist.Core.Interfaces;
using AimAssist.Service;
using AimAssist.ViewModels;
using Common.UI;
using Common.UI.Editor;
using Library.Options;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;

namespace AimAssist.UI.PickerWindows
{
    public partial class PickerWindow : Window
    {
        private readonly PickerWindowViewModel _viewModel;
        private readonly IApplicationLogService _logService;
        private readonly KeySequenceManager _keySequenceManager;
        private readonly IEditorOptionService _editorOptionService;

        public string SnippetText => _viewModel.SnippetText;
        public Common.Commands.Shortcus.KeySequence KeySequence => _viewModel.KeySequence;

        public PickerWindow(
            string processName, 
            ICommandService commandService, 
            IUnitsService unitsService, 
            IEditorOptionService editorOptionService,
            IApplicationLogService logService)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _editorOptionService = editorOptionService ?? throw new ArgumentNullException(nameof(editorOptionService));
            _keySequenceManager = new KeySequenceManager(commandService);

            _viewModel = new PickerWindowViewModel(processName, unitsService, editorOptionService);
            
            InitializeComponent();
            
            DataContext = _viewModel;
            
            SourceInitialized += OnSourceInitialized;
            Closing += OnClosing;
            Deactivated += OnDeactivated;
            
            InitializeEditor();
            FilterTextBox.Focus();
        }

        private void InitializeEditor()
        {
            try
            {
                var editor = EditorCache.Editor;
                if (editor != null)
                {
                    MainContent.Content = editor;
                    editor.SetOption(_editorOptionService.Option);
                }
                else
                {
                    var monacoEditor = new MonacoEditor();
                    MainContent.Content = monacoEditor;
                    EditorCache.Editor = monacoEditor;
                    monacoEditor.SetOption(_editorOptionService.Option);
                }
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, "エディタ初期化中にエラーが発生しました");
            }
        }

        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            try
            {
                var handle = new WindowInteropHelper(this).Handle;
                HwndSource.FromHwnd(handle)?.AddHook(WndProc);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, "ウィンドウ初期化中にエラーが発生しました");
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_SYSKEYDOWN = 0x0104;
            const int VK_MENU = 0x12;

            if (msg == WM_SYSKEYDOWN && wParam.ToInt32() == VK_MENU)
            {
                handled = true;
                return IntPtr.Zero;
            }

            return IntPtr.Zero;
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            try
            {
                _viewModel.IsClosing = true;
                if (MainContent.Content is MonacoEditor editor)
                {
                    EditorCache.Editor = editor;
                }
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, "ウィンドウ終了処理中にエラーが発生しました");
            }
        }

        private void OnDeactivated(object? sender, EventArgs e)
        {
            // フォーカス喪失時の処理（必要に応じて実装）
        }

        public void FocusContent()
        {
            if (MainContent.Content is IFocasable focusable)
            {
                focusable.Focus();
            }
        }
    }
}
