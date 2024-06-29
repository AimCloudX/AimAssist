using AimAssist.Core.Events;
using Common.Commands;
using Common.Commands.Shortcus;
using System.Diagnostics;
using System.Windows.Input;

namespace AimAssist.Core.Commands
{
    public static class CommandService
    {
        private static Dictionary<string, KeySequence> keymap = new Dictionary<string, KeySequence>();

        private static List<RelayCommand> dic = new List<RelayCommand>();

        public static void SetKeymap(Dictionary<string, KeySequence> maps)
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

        public static Dictionary<string, KeySequence> GetKeymap()
        {
            return keymap;
        }

        public static bool TryGetFirstOnlyKeyCommand(KeyGesture keyGesture, out RelayCommand command)
        {
            foreach(var keyValuePair in keymap)
            {
                var keySequence = keyValuePair.Value;
                if (keySequence.FirstKey == keyGesture.Key && keySequence.FirstModifiers == keyGesture.Modifiers && keySequence.SecondKey == null && keySequence.SecondModifiers == null)
                {
                    if(TryGetCommand(keyValuePair.Key, out command))
                    {
                        return true;
                    }
                }
            }

            command = null;
            return false;
        }

        public static bool TryGetFirstSecontKeyCommand(KeySequence input, out RelayCommand command)
        {
            foreach(var keyValuePair in keymap)
            {
                var keySequence = keyValuePair.Value;
                if (keySequence.FirstKey == input.FirstKey && keySequence.FirstModifiers == input.FirstModifiers && keySequence.SecondKey == input.SecondKey && keySequence.SecondModifiers == input.SecondModifiers)
                {
                    if(TryGetCommand(keyValuePair.Key, out command))
                    {
                        return true;
                    }
                }
            }

            command = null;
            return false;
        }


        public static void Register(RelayCommand command, KeySequence defaultKeyMap)
        {
            if(dic.Any(x=>x == command))
            {
                Debug.Assert(false,"同名のコマンドがすでに登録されています");
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

        public static bool TryGetCommand(string commandName, out RelayCommand command)
        {
            command = dic.FirstOrDefault(x => x.CommandName == commandName);
            return command != null;
        }

        public static void UpdateKeyGesture(string commandName, KeySequence key)
        {
            if(!TryGetCommand(commandName, out var command))
            {
                return;
            }

            if (keymap.TryGetValue(commandName, out var before))
            {
                keymap[commandName] = key;
                if(command is HotkeyCommand hotkeyCommand)
                {
                    EventPublisher.KeyUpdateEventPublisher.RaiseEvent(hotkeyCommand, before, key);
                }
            }
        }

        public static bool TryGetKeyGesutre(string commandName, out RelayCommand command, out KeySequence keyGesture)
        {
            if(TryGetCommand(commandName, out command))
            {
                 keyGesture = keymap[commandName];
                return true;
            }

            keyGesture = null;
            return false;
        }

        public static void Execute(string commandName)
        {
            var command = dic.FirstOrDefault(x => x.CommandName == commandName);
            if(command != null)
            {
                command.Execute();
            }
        }
    }
}
