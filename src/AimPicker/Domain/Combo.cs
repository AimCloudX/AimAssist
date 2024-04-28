using System;

namespace AimPicker.Domain
{
    public class Combo
    {
        public Combo(string name, string text)
        {
            Name = name;
            Snippet = text;
        }

        public string Name { get; }

        public string Snippet { get; }
    }
}
