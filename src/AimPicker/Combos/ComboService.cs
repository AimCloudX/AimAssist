using AimPicker.Combos.Mode.Snippet;
using AimPicker.Combos.Mode.WorkFlows;
using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.Bookmarks;
using AimPicker.Unit.Implementation.BookSearch;
using AimPicker.Unit.Implementation.Snippets;
using AimPicker.Unit.Implementation.Urls;
using AimPicker.Unit.Implementation.Wiki;
using AimPicker.Unit.Implementation.WorkFlows;
using Common.UI;

namespace AimPicker.Combos
{
    public static class ComboService
    {
        public static Dictionary<IPickerMode, List<IUnit>> ComboDictionary2 = new Dictionary<IPickerMode, List<IUnit>>() {
            { SnippetMode.Instance, new List<IUnit>()
            {
            new SnippetCombo("aim","AimNext"),
            new SnippetCombo("Today",DateTime.Now.ToString("d")),
            new SnippetCombo("Now",DateTime.Now.ToString("t")),
            new SnippetCombo("AppData",Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)),
            new SnippetCombo("Downloads",Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads")),
            new SnippetCombo("環境変数","control.exe sysdm.cpl,,3"),
            }
    },{WorkFlowMode.Instance, new List<IUnit>{
        new WorkFlowCombo("ChatGPT", "https://chatgpt.com/", new WebViewPreviewFactory()),
    } }
        };

        public static List<IPickerMode> ModeLists = new List<IPickerMode>() {
            {NormalMode.Instance },
            {SnippetMode.Instance },
            {WorkFlowMode.Instance },
            {BookSearchMode.Instance },
            {CalculationMode.Instance },
            {BookmarkMode.Instance },
            {UrlMode.Instance },
            {KnowledgeMode.Instance },
        };

        public static List<ModeUnit> ModeComboLists = new List<ModeUnit>() {
            {new ModeUnit(SnippetMode.Instance) },
            {new ModeUnit(WorkFlowMode.Instance) },
            {new ModeUnit(BookSearchMode.Instance) },
            {new ModeUnit(CalculationMode.Instance) },
            {new ModeUnit(BookmarkMode.Instance) },
            {new ModeUnit(KnowledgeMode.Instance) },
        };
    }
}