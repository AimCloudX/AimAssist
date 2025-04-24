using AimAssist.Core.Units;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.WorkTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Units.Implementation.Pdf
{
    public class PdfMergeUnit : IUnit
    {
        public IMode Mode => WorkToolsMode.Instance;

        public string Name => "PDFの結合";

        public string Description => "PDFの結合";

        public string Category => "";
    }
}
