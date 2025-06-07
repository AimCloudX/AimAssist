using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Services.Editors;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.Options;
using Common.UI.Markdown;
using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.UI.UnitContentsView.ViewProviders
{
    [ViewProvider(Priority = 90)]
    public class FileBasedViewProvider : IViewProvider
    {
        public int Priority => 90;

        public bool CanProvideView(Type unitType) =>
            unitType == typeof(MarkdownUnit) ||
            unitType == typeof(EditorUnit) ||
            unitType == typeof(OptionUnit);

        public UIElement CreateView(IUnit unit, IServiceProvider serviceProvider)
        {
            return unit switch
            {
                MarkdownUnit md => new MarkdownView(md.FullPath),
                EditorUnit editor => CreateEditor(editor.FullPath, serviceProvider),
                OptionUnit option => CreateMultiFileEditor(option.OptionFilePaths, serviceProvider),
                _ => null
            };
        }

        private UIElement CreateEditor(string filePath, IServiceProvider serviceProvider)
        {
            var editorOptionService = serviceProvider.GetService<IEditorOptionService>();
            var editor = new AimEditor(editorOptionService);
            editor.NewTab(filePath);
            return editor;
        }

        private UIElement CreateMultiFileEditor(IEnumerable<string> filePaths, IServiceProvider serviceProvider)
        {
            var editorOptionService = serviceProvider.GetService<IEditorOptionService>();
            var editor = new AimEditor(editorOptionService);

            foreach (var filePath in filePaths)
            {
                editor.NewTab(filePath);
            }

            return editor;
        }
    }
}