using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AimPicker.Combos;
using AimPicker.Unit.Core;

namespace AimPicker.Combos.Mode.Snippet
{
    public class SnippetCombo : IUnit
    {
        public SnippetCombo(string name, string text)
        {
            Name = name;
            Text = text;

        }

        public string Name { get; }

        public string Text { get; }

        public UIElement PreviewUI => throw new NotImplementedException();
    }
}
