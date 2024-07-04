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
            yield return new SnippetUnit("aim", "AimNext", "Aim");
            yield return new SnippetUnit("Today", DateTime.Now.ToString("d"), "DateTime");
            yield return new SnippetUnit("Now", DateTime.Now.ToString("d"), "DateTime");
            yield return new SnippetUnit("AppData", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            yield return new SnippetUnit("Downloads", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads"));
            yield return new SnippetUnit("環境変数", "control.exe sysdm.cpl,,3");
            if (Clipboard.ContainsText())
            {
                yield return new SnippetUnit("クリップボード", Clipboard.GetText());
            }
        }
    }
}
