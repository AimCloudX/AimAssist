using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimPicker.DomainModels
{
    public class SnippetCombo : ICombo
    {
        public SnippetCombo(string name, string Description)
        {
            this.Name  = name;
            this.Code = Description;

        }

        public string Name { get; }

        public string Code { get; }
    }
}
