using System.Windows.Input;

namespace Common.UI.Commands.Shortcus
{
    public static class KeySequenceParser
{
    public static List<KeySequenceItem> Parse(string markdown, string applicationName)
    {
        var lines = markdown.Split('\n');
        var result = new List<KeySequenceItem>();
        string currentTitle = "";

        foreach (var line in lines)
        {
            if (line.StartsWith("#"))
            {
                currentTitle = line.TrimStart('#', ' ');
            }
            else if (line.StartsWith("-"))
            {
                var parts = line.TrimStart('-', ' ').Split(new[] { ' ' }, 2);
                if (parts.Length == 2)
                {
                        var operation = parts[0];
                        var keysequence = ParseKeySequence(parts[1].Trim());
                        result.Add(new KeySequenceItem(keysequence, operation, currentTitle, applicationName));
                }
            }
        }

        return result;
    }

    private static KeySequence ParseKeySequence(string sequence)
    {
        var parts = sequence.Split('+', ' ').Select(p => p.Trim()).ToArray();
        
        Key? firstKey = null;
        ModifierKeys firstModifiers = ModifierKeys.None;
        Key? secondKey = null;
        ModifierKeys secondModifiers = ModifierKeys.None;

        for (int i = 0; i < parts.Length; i++)
        {
            if (i == parts.Length - 1 || (i == parts.Length - 3 && parts[i + 1] == "+"))
            {
                if (firstKey == null)
                {
                    firstKey = ParseKey(parts[i]);
                }
                else
                {
                    secondKey = ParseKey(parts[i]);
                }
            }
            else
            {
                if (firstKey == null)
                {
                    firstModifiers |= ParseModifier(parts[i]);
                }
                else
                {
                    secondModifiers |= ParseModifier(parts[i]);
                }
            }
        }

        return new KeySequence(firstKey ?? Key.None, firstModifiers, secondKey, secondModifiers);
    }

    private static Key ParseKey(string key)
    {
        if (Enum.TryParse<Key>(key, true, out var result))
        {
            return result;
        }
        return Key.None;
    }

    private static ModifierKeys ParseModifier(string modifier)
    {
        switch (modifier.ToLower())
        {
            case "ctrl":
            case "control":
                return ModifierKeys.Control;
            case "alt":
                return ModifierKeys.Alt;
            case "shift":
                return ModifierKeys.Shift;
            case "win":
            case "windows":
                return ModifierKeys.Windows;
            default:
                return ModifierKeys.None;
        }
    }
}
}
