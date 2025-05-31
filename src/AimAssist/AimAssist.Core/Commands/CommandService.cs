using AimAssist.Core.Events;
using AimAssist.Core.Interfaces;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Common.UI.Commands;
using Common.UI.Commands.Shortcus;

namespace AimAssist.Core.Commands
{
    /// <summary>
    /// コマンドサービスの実装クラス
    /// </summary>
    public class CommandService : ICommandService
    {
        private Dictionary<string, KeySequence> keymap = new Dictionary<string, KeySequence>();
        private List<RelayCommand> dic = new List<RelayCommand>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CommandService()
        {
            // DIでインスタンス化されるように、パブリックコンストラクタを提供
        }

        /// <summary>
        /// キーマップを設定します
        /// </summary>
        /// <param name="maps">キーマップのディクショナリ</param>
        public void SetKeymap(Dictionary<string, KeySequence> maps)
        {
            foreach (var map in maps)
            {
                if (keymap.TryGetValue(map.Key, out var value))
                {
                    keymap[map.Key] = map.Value;
                }
                else
                {
                    keymap.Add(map.Key, map.Value);
                }
            }
        }

        /// <summary>
        /// 現在のキーマップを取得します
        /// </summary>
        /// <returns>キーマップのディクショナリ</returns>
        public Dictionary<string, KeySequence> GetKeymap()
        {
            return keymap;
        }

        /// <summary>
        /// 最初のキーのみのコマンドを取得します
        /// </summary>
        /// <param name="keyGesture">キージェスチャー</param>
        /// <param name="command">取得したコマンド</param>
        /// <returns>コマンドが見つかった場合はtrue、それ以外はfalse</returns>
        public bool TryGetFirstOnlyKeyCommand(KeyGesture keyGesture, out RelayCommand command)
        {
            foreach (var keyValuePair in keymap)
            {
                var keySequence = keyValuePair.Value;
                if (keySequence.FirstKey == keyGesture.Key && keySequence.FirstModifiers == keyGesture.Modifiers && keySequence.SecondKey == null && keySequence.SecondModifiers == null)
                {
                    if (TryGetCommand(keyValuePair.Key, out command))
                    {
                        return true;
                    }
                }
            }

            command = null;
            return false;
        }

        /// <summary>
        /// 1次、2次キーを使用するコマンドを取得します
        /// </summary>
        /// <param name="input">入力されたキーシーケンス</param>
        /// <param name="command">取得したコマンド</param>
        /// <returns>コマンドが見つかった場合はtrue、それ以外はfalse</returns>
        public bool TryGetFirstSecontKeyCommand(KeySequence input, out RelayCommand command)
        {
            foreach (var keyValuePair in keymap)
            {
                var keySequence = keyValuePair.Value;
                if (keySequence.FirstKey == input.FirstKey && keySequence.FirstModifiers == input.FirstModifiers && keySequence.SecondKey == input.SecondKey && keySequence.SecondModifiers == input.SecondModifiers)
                {
                    if (TryGetCommand(keyValuePair.Key, out command))
                    {
                        return true;
                    }
                }
            }

            command = null;
            return false;
        }

        /// <summary>
        /// コマンドを登録します
        /// </summary>
        /// <param name="command">登録するコマンド</param>
        /// <param name="defaultKeyMap">デフォルトのキーマップ</param>
        public void Register(RelayCommand command, KeySequence defaultKeyMap)
        {
            if (dic.Any(x => x == command))
            {
                Debug.Assert(false, "同名のコマンドがすでに登録されています");
                return;
            }

            if (keymap.TryGetValue(command.CommandName, out _))
            {
                //keymap[command.CommandName] = defaultKeyMap;
            }
            else
            {
                keymap.Add(command.CommandName, defaultKeyMap);
            }

            dic.Add(command);
        }

        /// <summary>
        /// コマンド名からコマンドを取得します
        /// </summary>
        /// <param name="commandName">コマンド名</param>
        /// <param name="command">取得したコマンド</param>
        /// <returns>コマンドが見つかった場合はtrue、それ以外はfalse</returns>
        public bool TryGetCommand(string commandName, out RelayCommand command)
        {
            command = dic.FirstOrDefault(x => x.CommandName == commandName);
            return command != null;
        }

        /// <summary>
        /// キージェスチャーを更新します
        /// </summary>
        /// <param name="commandName">コマンド名</param>
        /// <param name="key">新しいキーシーケンス</param>
        public void UpdateKeyGesture(string commandName, KeySequence key)
        {
            if (!TryGetCommand(commandName, out var command))
            {
                return;
            }

            if (keymap.TryGetValue(commandName, out var before))
            {
                keymap[commandName] = key;
                if (command is HotkeyCommand hotkeyCommand)
                {
                    EventPublisher.KeyUpdateEventPublisher.RaiseEvent(hotkeyCommand, before, key);
                }
            }
        }

        /// <summary>
        /// コマンド名からコマンドとキージェスチャーを取得します
        /// </summary>
        /// <param name="commandName">コマンド名</param>
        /// <param name="command">取得したコマンド</param>
        /// <param name="keyGesture">関連するキージェスチャー</param>
        /// <returns>コマンドが見つかった場合はtrue、それ以外はfalse</returns>
        public bool TryGetKeyGesutre(string commandName, out RelayCommand command, out KeySequence keyGesture)
        {
            if (TryGetCommand(commandName, out command))
            {
                keyGesture = keymap[commandName];
                return true;
            }

            keyGesture = null;
            return false;
        }

        /// <summary>
        /// コマンドを実行します
        /// </summary>
        /// <param name="commandName">コマンド名</param>
        /// <param name="window">実行対象のウィンドウ</param>
        public void Execute(string commandName, Window window)
        {
            var command = dic.FirstOrDefault(x => x.CommandName == commandName);
            if (command != null)
            {
                command.Execute(window);
            }
        }
    }
}
