using AimAssist.Combos.Mode.Snippet;
using AimAssist.Units.Core;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Implementation.Snippets
{
    public class SnippetUnitsFactory : IUnitsFacotry
    {
        public IMode TargetMode => SnippetMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits()
        {
            // TODO:設定ファイルからのLoad
            yield return new Unit(TargetMode, "aim", new SnippetModel("aim", "AimNext","Aim"));
            yield return new Unit(TargetMode, "Today", "DateTime", new SnippetModel("Today", DateTime.Now.ToString("d"),"DateTime"));
            yield return new Unit(TargetMode, "Now", new SnippetModel("Now", DateTime.Now.ToString("d")));
            yield return new Unit(TargetMode, "AppData", new SnippetModel("AppData", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));
            yield return new Unit(TargetMode, "Downloads", new SnippetModel("Downloads", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads")));
            yield return new Unit(TargetMode, "環境変数", new SnippetModel("環境変数", "control.exe sysdm.cpl,,3"));
        }
    }
}
