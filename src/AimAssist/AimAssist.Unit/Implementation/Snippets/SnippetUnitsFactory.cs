using AimAssist.Units.Core;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;
using System.Windows;

namespace AimAssist.Units.Implementation.Snippets
{
    public class SnippetUnitsFactory : IUnitsFacotry
    {
        public IMode TargetMode => SnippetMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits()
        {
            // TODO:設定ファイルからのLoad
            yield return SnippetUnit.Create("aim", "AimNext","Aim");
            yield return SnippetUnit.Create("Today", DateTime.Now.ToString("d"),"DateTime");
            yield return SnippetUnit.Create("Now",DateTime.Now.ToString("d"), "DateTime");
            yield return SnippetUnit.Create("AppData", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            yield return SnippetUnit.Create("Downloads", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads"));
            yield return SnippetUnit.Create("環境変数","control.exe sysdm.cpl,,3");
            if (Clipboard.ContainsText())
            {
                yield return SnippetUnit.Create("クリップボード", Clipboard.GetText());
            }
        }
    }
}
