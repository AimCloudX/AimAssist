using AimAssist.Core.Units;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Implementation.Snippets
{
    public class SnippetUnit : IUnit
    {
        public SnippetUnit(string name, string text, string category = "")
        {
            Name = name;
            Code = text;
            Category = category;
        }

        public string Name { get; }

        public string Code { get; }

        public string Category { get; }

        public IMode Mode => SnippetMode.Instance;

        public string Description => string.Empty;
    }
    public class SnippetModelUnit :IUnit
    {
        public SnippetModelUnit(Snippet snippet)
        {
            Name = snippet.Name;
            Code = snippet.Content;
            Category = snippet.Category;
        }

        public string Name { get; }

        public string Code { get; }

        public string Category { get; }

        public IMode Mode => SnippetMode.Instance;

        public string Description => string.Empty;
    }
}
