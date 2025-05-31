using AimAssist.Core.Units;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.CodeGenarator;
using AimAssist.Units.Implementation.Computer;
using AimAssist.Units.Implementation.Pdf;
using AimAssist.Units.Implementation.Web.MindMeister;
using AimAssist.Units.Implementation.Web.Rss;

namespace AimAssist.Units.Implementation.Factories
{
    public interface ICoreUnitsFactory
    {
        IEnumerable<IUnit> CreateUnits();
    }

    public class CoreUnitsFactory : ICoreUnitsFactory
    {
        public IEnumerable<IUnit> CreateUnits()
        {
            yield return new CodeGeneratorUnit();
            yield return new TranscriptionUnit();
            yield return new PdfMergeUnit();
            yield return new RssSettingUnit();
            yield return new ComputerUnit();
            yield return new MindMeisterUnit("最近開いたMap", "https://www.mindmeister.com/app/maps/recent");
        }
    }
}
