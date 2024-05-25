using AimPicker.Domain;
using AimPicker.DomainModels;
using AimPicker.UI.Combos;
using AimPicker.UI.Combos.Commands;
using AimPicker.UI.Combos.Snippets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AimPicker.Service
{
    public static class ComboService
    {
        public static Dictionary<IPickerMode, List<ICombo>> ComboDictionary2 = new Dictionary<IPickerMode, List<ICombo>>() {
            { SnippetMode.Instance, new List<ICombo>()
            {
            new SnippetCombo("aim","AimNext"),
            new SnippetCombo("Today",DateTime.Now.ToString("d")),
            new SnippetCombo("Now",DateTime.Now.ToString("t")),
            new SnippetCombo("AppData",Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)),
            new SnippetCombo("Downloads",Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads")),
            new SnippetCombo("環境変数","control.exe sysdm.cpl,,3"),
            }
    },{WorkFlowMode.Instance, new List<ICombo>{
        new WorkFlowCombo("ChatGPT", "https://chatgpt.com/", new WebViewPreviewFactory())
    } }
        };
        public static Dictionary<IPickerMode, List<IComboViewModel>> ComboDictionary = new Dictionary<IPickerMode, List<IComboViewModel>>() {
            { SnippetMode.Instance, new List<IComboViewModel>()
            {
            new SnippetViewModel("aim","AimNext"),
            new SnippetViewModel("Today",DateTime.Now.ToString("d")),
            new SnippetViewModel("Now",DateTime.Now.ToString("t")),
            new SnippetViewModel("AppData",Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)),
            new SnippetViewModel("Downloads",Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads")),
            new SnippetViewModel("環境変数","control.exe sysdm.cpl,,3"),
            }
    },{WorkFlowMode.Instance, new List<IComboViewModel>{
        new PickerCommandViewModel("ChatGPT", "https://chatgpt.com/", new WebViewPreviewFactory())

    } }
        };


    }
}