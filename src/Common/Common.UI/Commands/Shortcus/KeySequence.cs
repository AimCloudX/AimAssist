using System.Text.Json.Serialization;
using System.Windows.Input;

namespace Common.Commands.Shortcus
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
    }
}
