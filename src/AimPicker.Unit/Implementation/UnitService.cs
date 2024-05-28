using AimPicker.Combos.Mode.Snippet;
using AimPicker.Combos.Mode.WorkFlows;
using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.Snippets;
using AimPicker.Unit.Implementation.Standard;
using AimPicker.Unit.Implementation.Web;
using AimPicker.Unit.Implementation.Web.Bookmarks;
using AimPicker.Unit.Implementation.Web.BookSearch;
using AimPicker.Unit.Implementation.Web.Urls;
using AimPicker.Unit.Implementation.Wiki;
using AimPicker.Unit.Implementation.WorkFlows;

namespace AimPicker.Service
{
    public static class UnitService
    {
        public static Dictionary<IPickerMode, List<IUnit>> UnitDictionary = new Dictionary<IPickerMode, List<IUnit>>() {
            { SnippetMode.Instance, new List<IUnit>()
            {
            new SnippetUnit("aim","AimNext"),
            new SnippetUnit("Today",DateTime.Now.ToString("d")),
            new SnippetUnit("Now",DateTime.Now.ToString("t")),
            new SnippetUnit("AppData",Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)),
            new SnippetUnit("Downloads",Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads")),
            new SnippetUnit("環境変数","control.exe sysdm.cpl,,3"),
            }
    },{WorkFlowMode.Instance, new List<IUnit>{
        new WorkFlowCombo("ChatGPT", "https://chatgpt.com/", new WebViewPreviewFactory()),
    } }
        };

        public static List<IPickerMode> ModeLists = new List<IPickerMode>() {
            {StandardMode.Instance },
            {SnippetMode.Instance },
            {WorkFlowMode.Instance },
            {BookSearchMode.Instance },
            {CalculationMode.Instance },
            {BookmarkMode.Instance },
            {UrlMode.Instance },
            {KnowledgeMode.Instance },
        };

        public static IPickerMode GetModeFromText(string text)
        {
            foreach (var mode in ModeLists.Where(x => !string.IsNullOrEmpty(x.Prefix)))
            {
                if (text.StartsWith(mode.Prefix))
                {
                    return mode;
                }
            }

            return StandardMode.Instance;
        }

    }
}