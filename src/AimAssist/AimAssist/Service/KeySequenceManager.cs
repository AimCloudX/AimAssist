using AimAssist.Core.Commands;
using AimAssist.Core.Interfaces;
using System.Windows;
using System.Windows.Input;
using Common.UI.Commands.Shortcus;

namespace AimAssist.Service
{
    public class KeySequenceManager : IKeySequenceManager
    {
        private Key _lastKey;
        private ModifierKeys _lastModifiers;
        private DateTime _lastKeyPressTime;
        private bool _isWaitingForSecondKey = false;
        private readonly ICommandService _commandService;

        public KeySequenceManager(ICommandService commandService)
        {
            _commandService = commandService;
        }

        public bool HandleKeyPress(Key key, ModifierKeys modifiers, Window window)
        {
            var now = DateTime.Now;

            if (IsModifierKeyOnly(key) || modifiers == ModifierKeys.None|| modifiers == ModifierKeys.Shift)
            {
                return false;
            }

            if (_isWaitingForSecondKey && (now - _lastKeyPressTime).TotalMilliseconds <= 500)
            {
                // 2つ目のキーを処理
                var keySequence = new KeySequence(_lastKey, _lastModifiers, key, modifiers);
                if (_commandService.TryGetFirstSecontKeyCommand(keySequence, out var doubleKeyCommand))
                {
                    doubleKeyCommand.Execute(window);
                    ResetKeySequence();
                    return true;
                }
                else
                {
                    ResetKeySequence();
                    return false;
                }
            }

            // 1つのキーのシーケンスのチェック
            var singleKeySequence = new KeyGesture(key, modifiers);
            if (_commandService.TryGetFirstOnlyKeyCommand(singleKeySequence, out var command))
            {
                command.Execute(window);
                ResetKeySequence();
                return true;
            }

            // 2つのキーシーケンスの最初のキーとして記録
            _lastKey = key;
            _lastModifiers = modifiers;
            _lastKeyPressTime = now;
            _isWaitingForSecondKey = true;

            return false;
        }

        private void ResetKeySequence()
        {
            _lastKey = Key.None;
            _lastModifiers = ModifierKeys.None;
            _lastKeyPressTime = DateTime.MinValue;
            _isWaitingForSecondKey = false;
        }

        private bool IsModifierKeyOnly(Key key)
        {
            return key == Key.LeftCtrl || key == Key.RightCtrl ||
                   key == Key.LeftAlt || key == Key.RightAlt ||
                   key == Key.LeftShift || key == Key.RightShift ||
                   key == Key.LWin || key == Key.RWin;
        }
    }
}
