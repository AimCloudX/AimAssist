using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace Common
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

        public override string ToString()
        {
            if (IsSingleKeySequence)
            {
                return $"{FirstModifiers}+{FirstKey}";
            }
            return $"{FirstModifiers}+{FirstKey}, {SecondModifiers}+{SecondKey}";
        }
    }
}
