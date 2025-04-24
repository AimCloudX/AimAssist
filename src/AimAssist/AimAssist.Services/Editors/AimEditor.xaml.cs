using AimAssist.Core.Interfaces;
using Library.Editors;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;

namespace AimAssist.Core.Editors
{
    /// <summary>
    /// AimEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class AimEditor : UserControl
    {
        public ObservableCollection<FileModel> Models { get; } = new ObservableCollection<FileModel>();
        private readonly IEditorOptionService _editorOptionService;

        public AimEditor(IEditorOptionService editorOptionService)
        {
            _editorOptionService = editorOptionService;
            InitializeComponent();
            this.DataContext = this;
        }

        public void NewTab(string filePath)
        {
            var model = new FileModel(filePath, _editorOptionService);
            Models.Add(model);
        }

        public async void Save()
        {
            var tab = Models.FirstOrDefault(x => x.IsSelected);
            if(tab is FileModel model) {
                var  text = await model.monacoEditor.GetText();
                File.WriteAllText(model.FilePath, text);
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Save();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var firstOrDefault = this.Models.FirstOrDefault(x => x.IsSelected);
            if(firstOrDefault != null)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = firstOrDefault.FilePath,
                    UseShellExecute = true
                }); 
            }

        }
    }
}
