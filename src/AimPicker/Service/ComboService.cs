﻿using AimPicker.Domain;
using AimPicker.DomainModels;
using AimPicker.UI.Combos.Commands;
using Microsoft.Web.WebView2.Core;

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
            {WikiMode.Instance },
        };

        public static List<ModeCombo> ModeComboLists = new List<ModeCombo>() {
            {new ModeCombo(SnippetMode.Instance) },
            {new ModeCombo(WorkFlowMode.Instance) },
            {new ModeCombo(BookSearchMode.Instance) },
            {new ModeCombo(CalculationMode.Instance) },
            {new ModeCombo(BookmarkMode.Instance) },
            {new ModeCombo(WikiMode.Instance) },
        };
    }
}