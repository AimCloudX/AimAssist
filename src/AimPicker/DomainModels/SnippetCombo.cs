using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimPicker.DomainModels
{
    public class SnippetCombo : ICombo
    {
        public SnippetCombo(string name, string text)
        {
            this.Name  = name;
            this.Text = text;

        }

        public string Name { get; }

        public string Text { get; }
    }
}
