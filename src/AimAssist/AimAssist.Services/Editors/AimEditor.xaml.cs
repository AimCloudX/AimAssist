﻿using Library.Editors;
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

        public AimEditor()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public void NewTab(string filePath)
        {
            var model = new FileModel(filePath);
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
    }
}