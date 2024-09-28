using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.WorkTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Units.Implementation.CodeGenarator
{
    public class CodeGeneratorUnit : IUnit
    {
        public IMode Mode => WorkToolsMode.Instance;

        public string Name => "Code Generator";

        public string Description => "Gemini APIを使ったコード生成";

        public string Category => "AIツール";
    }
}
