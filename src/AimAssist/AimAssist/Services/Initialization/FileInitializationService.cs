using System.IO;
using AimAssist.Core.Interfaces;

namespace AimAssist.Services.Initialization
{
    public interface IFileInitializationService
    {
        void InitializeFiles();
    }
    
    public class FileInitializationService : IFileInitializationService
    {
        private readonly ISnippetOptionService _snippetOptionService;
        private readonly IWorkItemOptionService _workItemOptionService;
        private readonly IEditorOptionService _editorOptionService;
        private readonly IApplicationLogService _logService;

        public FileInitializationService(
            ISnippetOptionService snippetOptionService,
            IWorkItemOptionService workItemOptionService,
            IEditorOptionService editorOptionService,
            IApplicationLogService logService)
        {
            _snippetOptionService = snippetOptionService;
            _workItemOptionService = workItemOptionService;
            _editorOptionService = editorOptionService;
            _logService = logService;
        }

        public void InitializeFiles()
        {
            InitializeEditorFiles();
            InitializeWorkItemFiles();
            InitializeSnippetFiles();
            LoadOptions();
        }

        private void InitializeEditorFiles()
        {
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string editorOptionPath = Path.Combine(roamingPath, "AimAssist", "editor.option.json");
            string editorOptionSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Settings", "editor.option.json");
            
            if (!File.Exists(editorOptionPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(editorOptionPath)!);
                File.Copy(editorOptionSource, editorOptionPath);
            }
        }

        private void InitializeWorkItemFiles()
        {
            string workItemOptionSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "WorkItems", "workitem.option.json");
            if (!File.Exists(_workItemOptionService.OptionPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_workItemOptionService.OptionPath)!);
                File.Copy(workItemOptionSource, _workItemOptionService.OptionPath);
            }

            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string targetPath = Path.Combine(roamingPath, "AimAssist", "WorkItem.md");
            string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "WorkItems", "WorkItem.md");
            if (!File.Exists(targetPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                File.Copy(sourcePath, targetPath);
            }
        }

        private void InitializeSnippetFiles()
        {
            string snippetoption = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Snippets", "snippet.option.json");
            if (!File.Exists(_snippetOptionService.OptionPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_snippetOptionService.OptionPath)!);
                File.Copy(snippetoption, _snippetOptionService.OptionPath);
            }

            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string snippetDefault = Path.Combine(roamingPath, "AimAssist", "SnippetsStandard.md");
            string snippetSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Snippets", "SnippetsStandard.md");
            if (!File.Exists(snippetDefault))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(snippetDefault)!);
                File.Copy(snippetSource, snippetDefault);
            }
        }

        private void LoadOptions()
        {
            try
            {
                _workItemOptionService.LoadOption();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "WorkItemOptionServiceの初期化中にエラーが発生しました");
            }

            try
            {
                _snippetOptionService.LoadOption();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "SnippetOptionServiceの初期化中にエラーが発生しました");
            }

            try
            {
                _editorOptionService.LoadOption();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "EditorOptionServiceの初期化中にエラーが発生しました");
            }
        }
    }
}
