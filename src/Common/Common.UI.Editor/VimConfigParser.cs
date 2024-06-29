using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace Common.UI.Editor
{

public class VimConfigParser
{
    public static Dictionary<string, string> ParseVimrc(string filePath)
    {
        var keyBindings = new Dictionary<string, string>();

        foreach (var line in File.ReadLines(filePath))
        {
            if (line.StartsWith("map "))
            {
                var parts = line.Split(' ');
                if (parts.Length == 3)
                {
                    var before = parts[1];
                    var after = parts[2];
                    keyBindings[before] = after;
                }
            }
        }

        return keyBindings;
    }
}
}
