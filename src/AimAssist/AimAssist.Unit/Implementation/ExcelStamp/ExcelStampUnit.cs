using AimAssist.Core.Attributes;
using AimAssist.Core.Units;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.WorkTools;

namespace AimAssist.Units.Implementation.ExcelStamp
{
    [AutoRegisterUnit("Excel")]
    public class ExcelStampUnit : IUnit
    {
        public IMode Mode => WorkToolsMode.Instance;
        public string Name => "電子印鑑";
        public string Description => "Excelで使用する判子を作成してクリップボードにコピーします";
        public string Category => "Excel";
    }
}