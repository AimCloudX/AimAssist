using AimAssist.Core.Attributes;
using AimAssist.Core.Units;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.WorkTools;

namespace AimAssist.Units.Implementation.Pdf
{
    [AutoRegisterUnit("Document")]
    public class PdfMergeUnit : IUnit
    {
        public IMode Mode => WorkToolsMode.Instance;

        public string Name => "PDFの結合";

        public string Description => "PDFの結合";

        public string Category => "";
    }
}
