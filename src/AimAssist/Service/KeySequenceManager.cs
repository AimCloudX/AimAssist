using AimAssist.Core.Commands;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AimAssist.Service
{
    public class KeySequenceManager
    {
        private Key _lastKey;
        private ModifierKeys _lastModifiers;
        private DateTime _lastKeyPressTime;
        private bool _isWaitingForSecondKey = false;

        public bool HandleKeyPress(Key key, ModifierKeys modifiers)
        {
            var now = DateTime.Now;

            if (IsModifierKeyOnly(key) || IsIgnoredModifierCombination(modifiers) || modifiers == ModifierKeys.None)
            {
                return false;
            }

            if (_isWaitingForSecondKey && (now - _lastKeyPressTime).TotalMilliseconds <= 500)
            {
                // 2つ目のキーを処理
                var keySequence = new KeySequence(_lastKey, _lastModifiers, key, modifiers);
                if (CommandService.TryGetFirstSecontKeyCommand(keySequence, out var doubleKeyCommand))
                {
                    doubleKeyCommand.Execute(null);
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
            if (CommandService.TryGetFirstOnlyKeyCommand(singleKeySequence, out var command))
            {
                command.Execute(null);
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

        private bool IsIgnoredModifierCombination(ModifierKeys modifiers)
        {
            return false;
            // Ctrl+Alt の組み合わせを無視
            if ((modifiers & ModifierKeys.Control) != 0 && (modifiers & ModifierKeys.Alt) != 0)
            {
                return true;
            }

            // Ctrl+Shift の組み合わせを無視
            if ((modifiers & ModifierKeys.Control) != 0 && (modifiers & ModifierKeys.Shift) != 0)
            {
                return true;
            }

            // Shift+Alt の組み合わせを無視
            if ((modifiers & ModifierKeys.Shift) != 0 && (modifiers & ModifierKeys.Alt) != 0)
            {
                return true;
            }

            // Ctrl+Shift+Alt の組み合わせを無視
            if ((modifiers & ModifierKeys.Control) != 0 && (modifiers & ModifierKeys.Shift) != 0 && (modifiers & ModifierKeys.Alt) != 0)
            {
                return true;
            }

            return false;
        }
    }
}
