using AimAssist.Core.Events;
using AimAssist.UI;
using System.Diagnostics;
using System.Windows.Input;

namespace AimAssist.Core.Commands
{
    public static class CommandService
    {
        private static  KeyGestureValueSerializer serializer = new KeyGestureValueSerializer();


        private static Dictionary<RelayCommand, string> dic = new Dictionary<RelayCommand, string>();

        public static void Register(RelayCommand command, string gesture)
        {
            if(dic.ContainsKey(command))
            {
                Debug.Assert(false,"同名のコマンドがすでに登録されています");
                return;
            }

            dic.Add(command, gesture);
        }

        public static bool TryGetCommand(string commandName, out RelayCommand command)
        {
            command = dic.Keys.FirstOrDefault(x => x.CommandName == commandName);
            return command != null;
        }

        public static void UpdateKeyGesture(string commandName, string key)
        {
            if (TryGetCommand(commandName, out var command))
            {
                var before = dic[command];
                var beforeKeyGesture = (KeyGesture)serializer.ConvertFromString(before, null);

                dic[command] = key;
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
                var gesture = dic[command];
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
            command = dic.Keys.FirstOrDefault(x => x.CommandName == commandName);
            if(command != null)
            {
                gesture = dic[command];
            }
            else
            {
                gesture = string.Empty;
            }

            return command != null;
        }

        public static void Execute(string commandName)
        {
            var command = dic.Keys.FirstOrDefault(x => x.CommandName == commandName);
            if(command != null)
            {
                command.Execute();
            }
        }

        public static IReadOnlyDictionary<RelayCommand, string> Commands => dic;
    }
}
