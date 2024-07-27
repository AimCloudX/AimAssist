using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Units.Implementation.Snippets
{
    public class Snippet
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public string Category { get; set; }

        public Snippet(string name, string content, string category)
        {
            Name = name;
            Content = content;
            Category = category;
        }
    }
}
