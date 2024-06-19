using AimAssist.Core.Events;
using AimAssist.UI;
using System.Diagnostics;
using System.Windows.Input;

namespace AimAssist.Core.Commands
{
    public static class CommandService
    {
        private static  KeyGestureValueSerializer serializer = new KeyGestureValueSerializer();

        private static Dictionary<string, string> keymap = new Dictionary<string, string>();

        private static List<RelayCommand> dic = new List<RelayCommand>();

        public static void SetKeymap(Dictionary<string,string> maps)
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

    public static Dictionary<string, string> GetKeymap() {
            return keymap;
        }


        public static void Register(RelayCommand command, string defaultKeyMap)
        {
            if(dic.Any(x=>x == command))
            {
                Debug.Assert(false,"同名のコマンドがすでに登録されています");
                return;
            }

            if (keymap.TryGetValue(command.CommandName, out _))
            {
                keymap[command.CommandName] = defaultKeyMap;
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

        public static void UpdateKeyGesture(string commandName, string key)
        {
            if(!TryGetCommand(commandName, out var command))
            {
                return;
            }

            if (keymap.TryGetValue(commandName, out var before))
            {
                var beforeKeyGesture = (KeyGesture)serializer.ConvertFromString(before, null);

                keymap[commandName] = key;
                var after = (KeyGesture)serializer.ConvertFromString(key, null);

                if(command is HotkeyCommand hotkeyCommand)
                {
                    EventPublisher.KeyUpdateEventPublisher.RaiseEvent(hotkeyCommand, beforeKeyGesture, after);
                }
            }
        }

        public static bool TryGetKeyGesutre(string commandName, out RelayCommand command, out KeyGesture keyGesture)
        {
            if(TryGetCommand(commandName, out command))
            {
                var gesture = keymap[commandName];
                var serializer = new KeyGestureValueSerializer();
                var comvertFromString = serializer.ConvertFromString(gesture, null);
                if (comvertFromString is KeyGesture key)
                {
                    keyGesture = key;
                    return true;
                }
            }

            keyGesture = null;
            return false;
        }


        public static bool TryGetCommandAndGesture(string commandName, out RelayCommand command, out string gesture)
        {
            command = dic.FirstOrDefault(x => x.CommandName == commandName);
            if(command != null)
            {
                gesture = keymap[commandName];
            }
            else
            {
                gesture = string.Empty;
            }

            return command != null;
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
