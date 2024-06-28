using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Implementation.Snippets
{
    public class SnippetModel : IUnitContent
    {
        public SnippetModel(string name, string text, string category = "")
        {
            Name = name;
            Code = text;
            Category = category;
        }

        public string Name { get; }


        public string Code { get; }

        public string Category { get; }
    }
}
