using System.Text.Json.Serialization;
using System.Windows.Input;

namespace Common.UI.Commands.Shortcus
{
    public class KeySequence
    {
        public Key FirstKey { get; }
        public ModifierKeys FirstModifiers { get; }
        public Key? SecondKey { get; }
        public ModifierKeys? SecondModifiers { get; }

        [JsonConstructor]
        public KeySequence(Key firstKey, ModifierKeys firstModifiers, Key? secondKey = null, ModifierKeys? secondModifiers = null)
        {
            FirstKey = firstKey;
            FirstModifiers = firstModifiers;
            SecondKey = secondKey;
            SecondModifiers = secondModifiers;
        }

        public bool IsSingleKeySequence => SecondKey == null;

        public static KeySequence None => new KeySequence(Key.None, ModifierKeys.None);

        public override string ToString()
        {
            if (IsSingleKeySequence)
            {
                if (Equals(None))
                {
                    return string.Empty;
                }

                return $"{FirstModifiers}+{FirstKey}";
            }

            return $"{FirstModifiers}+{FirstKey}, {SecondModifiers}+{SecondKey}";
        }

        public bool Equals(KeySequence obj)
        {
            return obj.FirstKey == FirstKey && obj.FirstModifiers == FirstModifiers && obj.SecondKey == SecondKey && obj.SecondModifiers == SecondModifiers;
        }

        public override bool Equals(object? obj)
        {
            if (obj is KeySequence keySquence)
            {

                return Equals(keySquence);
            }

            return false;
        }
 public string Parse()
    {
        string result = "";

        // Helper function to add modifier keys
        string AddModifiers(ModifierKeys modifiers)
        {
            string modString = "";
            if ((modifiers & ModifierKeys.Control) != 0) modString += "^";
            if ((modifiers & ModifierKeys.Alt) != 0) modString += "%";
            if ((modifiers & ModifierKeys.Shift) != 0) modString += "+";
            if ((modifiers & ModifierKeys.Windows) != 0) modString += "^%";
            return modString;
        }

        // Helper function to convert Key to SendKeys format
        string ConvertKey(Key key)
        {
            return key switch
            {
                Key.Enter => "{ENTER}",
                Key.Tab => "{TAB}",
                Key.Escape => "{ESC}",
                Key.Back => "{BACKSPACE}",
                Key.Delete => "{DELETE}",
                Key.Up => "{UP}",
                Key.Down => "{DOWN}",
                Key.Left => "{LEFT}",
                Key.Right => "{RIGHT}",
                Key.Home => "{HOME}",
                Key.End => "{END}",
                Key.PageUp => "{PGUP}",
                Key.PageDown => "{PGDN}",
                Key.F1 => "{F1}",
                Key.F2 => "{F2}",
                Key.F3 => "{F3}",
                Key.F4 => "{F4}",
                Key.F5 => "{F5}",
                Key.F6 => "{F6}",
                Key.F7 => "{F7}",
                Key.F8 => "{F8}",
                Key.F9 => "{F9}",
                Key.F10 => "{F10}",
                Key.F11 => "{F11}",
                Key.F12 => "{F12}",
                Key.Add => "{ADD}",
                Key.Subtract => "{SUBTRACT}",
                Key.Multiply => "{MULTIPLY}",
                Key.Divide => "{DIVIDE}",
                _ => EscapeSpecialCharacters(key.ToString().ToLower())
            };
        }

        // Helper function to escape special characters
        string EscapeSpecialCharacters(string input)
        {
            string[] specialChars = { "+", "^", "%", "~", "(", ")", "[", "]", "{", "}" };
            foreach (var specialChar in specialChars)
            {
                if (input == specialChar)
                {
                    return "{" + input + "}";
                }
            }
            return input;
        }

        // Process first key
        result += AddModifiers(FirstModifiers);
        result += ConvertKey(FirstKey);

        // Process second key if present
        if (SecondKey.HasValue)
        {
            result += AddModifiers(SecondModifiers ?? ModifierKeys.None);
            result += ConvertKey(SecondKey.Value);
        }

        return result;
    }
    }
}
