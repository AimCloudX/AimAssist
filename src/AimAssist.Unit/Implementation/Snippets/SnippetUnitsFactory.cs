using AimAssist.Combos.Mode.Snippet;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;

namespace AimAssist.Unit.Implementation.Snippets
{
    public class SnippetUnitsFactory : IUnitsFacotry
    {
        public IMode TargetMode => SnippetMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            // TODO:設定ファイルからのLoad
            yield return new SnippetUnit("aim", "AimNext","Aim");
            yield return new SnippetUnit("Today", DateTime.Now.ToString("d"), "DateTime");
            yield return new SnippetUnit("Now", DateTime.Now.ToString("t"),"DateTime");
            yield return new SnippetUnit("AppData", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            yield return new SnippetUnit("Downloads",Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads"));
            yield return new SnippetUnit("環境変数", "control.exe sysdm.cpl,,3");
        }
    }
}
