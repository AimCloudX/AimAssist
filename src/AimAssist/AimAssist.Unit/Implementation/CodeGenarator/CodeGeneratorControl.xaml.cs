﻿//using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CodeGenerator
{
    public partial class CodeGeneratorControl : UserControl, INotifyPropertyChanged
    {
        public string RootFolder { get; set; }
        private string ApiKey;
        private string apiKeyFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "apikey.dat");
        private ApiService apiService;
        private CodeFileSaver codeFileSaver;
        private int versionCounter = 0;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<CodeVersion> CodeVersions { get; private set; } = new ObservableCollection<CodeVersion>();

        public ObservableCollection<string> InputFiles { get; } = new ObservableCollection<string>();
        public CodeGeneratorControl()
        {
            InitializeComponent();
            LoadApiKey();
            if(ApiKey  != null)
            {
                apiService = new ApiService(ApiKey);
            }

            codeFileSaver = new CodeFileSaver();
            InitializeLanguageComboBox();
            RootFolder = "C:\\";
            this.DataContext = this;
            UpdateFolderTreeView();
            var commandBindings = new CommandBinding();
            commandBindings.Command = Commands.AddFileCommand;
            commandBindings.Executed += CommandBindings_Executed;

            CommandBindings.Add(commandBindings);
        }

        private void CommandBindings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is not string filePath)
            {
                return;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            if (InputFiles.Any(x => x.Equals(filePath, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            InputFiles.Add(filePath);
        }

        private void LoadApiKey()
        {
            if (File.Exists(apiKeyFilePath))
            {
                try
                {
                    byte[] encryptedApiKey = File.ReadAllBytes(apiKeyFilePath);
                    byte[] apiKeyBytes = ProtectedData.Unprotect(encryptedApiKey, null, DataProtectionScope.CurrentUser);
                    string apiKey = Encoding.UTF8.GetString(apiKeyBytes);
                    this.ApiKey = apiKey;
                }
                catch (Exception ex)
                {
                    this.ApiKey = null;
                    StatusTextBlock.Text = $"APIキーの読み込みに失敗しました: {ex.Message}";
                }
            }
            else
            {
                this.ApiKey = null;
            }
        }

        private void SaveApiKey(string apiKey)
        {
            byte[] apiKeyBytes = Encoding.UTF8.GetBytes(apiKey);
            byte[] encryptedApiKey = ProtectedData.Protect(apiKeyBytes, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(apiKeyFilePath, encryptedApiKey);
        }

        private void MenuItem_InputApiKey_Click(object sender, RoutedEventArgs e)
        {
            var apiKeyWindow = new ApiKeyInputWindow();
            if (apiKeyWindow.ShowDialog() == true)
            {
                var apiKey = apiKeyWindow.ApiKey;
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    SaveApiKey(apiKey);
                    LoadApiKey();
                    if(ApiKey != null)
                    {
                        apiService = new ApiService(ApiKey);
                        StatusTextBlock.Text = "APIキーを保存しました。";
                    }
                    else
                    {
                        StatusTextBlock.Text = "APIキーが空です。";
                    }
                }
                else
                {
                    StatusTextBlock.Text = "APIキーが空です。";
                }
            }
        }

        private void SelectRootFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "ルートフォルダを選択"
            };

            if (dialog.ShowDialog() == true)
            {
                RootFolder = dialog.FolderName;
                OnPropertyChanged(nameof(RootFolder));
                UpdateFolderTreeView();
            }
        }

        private void UpdateFolderTreeView()
        {
            FolderTreeView.Items.Clear();
            var rootItem = new FolderTreeItem(new DirectoryInfo(RootFolder));
            rootItem.Items.Add(new TreeViewItem());
            rootItem.IsSelected = true;
            FolderTreeView.Items.Add(rootItem);
        }

        private void InitializeLanguageComboBox()
        {
            var languages = new List<string>
            {
                "TypeScript (React)",
                "TypeScript (Next.js)",
                "JavaScript",
                "bash",
                "fish",
                "bat",
                "C#",
                "C#+WPF",
                "ASP.NET Core",
                "Rust",
                "Python",
                "Json"
            };

            LanguageComboBox.ItemsSource = languages;
            LanguageComboBox.SelectedIndex = 0;
        }

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = $"コード生成開始";
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            string prompt = await PromptTextArea.GetTextAsync();
            string selectedLanguage = LanguageComboBox.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(prompt))
            {
                StatusTextBlock.Text = "プロンプトを入力してください。";
                return;
            }

            if(apiService == null)
            {
                MessageBox.Show("APIキーを設定してください");
                return;
            }

            try
            {
                var fileContent = new StringBuilder();
                foreach(var file in InputFiles)
                {
                    if (File.Exists(file))
                    {
                        var fileName  = Path.GetFileName(file);
                        var contents = File.ReadAllText(file);
                        fileContent.AppendLine(fileName);
                        fileContent.AppendLine(contents);
                    }
                }

                string generatedContent = await apiService.GenerateCodeWithGemini(prompt, selectedLanguage, fileContent.ToString());
                // バージョンを追加
                var newVersion = new CodeVersion
                {
                    VersionName = $"v{versionCounter++}",
                    Content = generatedContent
                };
                CodeVersions.Add(newVersion);
                VersionComboBox.SelectedItem = newVersion;

               await ResultTextEditor.SetTextAsync(generatedContent);
                StatusTextBlock.Text = $"コード生成完了";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"エラーが発生しました: {ex.Message}";
            }
            finally {
                Mouse.OverrideCursor = null;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var outputFolder = SaveFolderTextBlock.Text;
            if (string.IsNullOrEmpty(outputFolder))
            {
                StatusTextBlock.Text = "保存先フォルダを選択してください。";
                return;
            }

            string generatedContent = await ResultTextEditor.GetTextAsync();
            if (string.IsNullOrWhiteSpace(generatedContent))
            {
                StatusTextBlock.Text = "保存するコンテンツがありません。";
                return;
            }

            try
            {
                codeFileSaver.SaveGeneratedContent(generatedContent, outputFolder);
                StatusTextBlock.Text = $"ファイルが保存されました。";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"保存中にエラーが発生しました: {ex.Message}";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateFolderTreeView();
        }

        private void SetSyntaxHighlighting(string language)
        {
            //ResultTextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Markdown");
            // 言語に応じてシンタックスハイライトを設定
            // AvalonEditにはデフォルトでいくつかのハイライトが含まれています
            // 必要に応じてカスタムハイライトを追加できます
            //switch (language.ToLower())
            //{
            //    case "c#":
            //    case "csharp":
            //        ResultTextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            //        break;
            //    case "vb":
            //    case "vb.net":
            //        ResultTextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("VB.NET");
            //        break;
            //    // 他の言語に応じて追加
            //    default:
            //        break;
            //}
        }

        private void VersionComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (VersionComboBox.SelectedItem is string selectedVersion)
            {
                var version = CodeVersions.FirstOrDefault(v => v.VersionName == selectedVersion);
                if (version != null)
                {
                    //ResultTextEditor.Text = version.Content;
                    SetSyntaxHighlighting(LanguageComboBox.SelectedItem as string);
                    StatusTextBlock.Text = $"表示中: {selectedVersion}";
                }
            }
        }
        private void DiffButton_Click(object sender, RoutedEventArgs e)
        {
            if (CodeVersions.Count < 2)
            {
                System.Windows.MessageBox.Show("少なくとも2つのバージョンが必要です。", "差分確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var diffWindow = new DiffWindow(this);
            diffWindow.Show();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CodeVersions.Clear();
            versionCounter = 0;
            await this.ResultTextEditor.SetTextAsync(string.Empty);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var deleteItems = InputListBox.SelectedItems.OfType<string>().ToArray();
            foreach (var filePath in deleteItems)
            {
                InputFiles.Remove(filePath);
            }
        }

        private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(this.FolderTreeView.SelectedItem is FolderTreeItem folderTreeItem)
            {
                SaveFolderTextBlock.Text = folderTreeItem.Info.FullName;
            }

            if(this.FolderTreeView.SelectedItem is FileTreeItem fileTreeItem)
            {
                var directory  = Path.GetDirectoryName(fileTreeItem.Info.FullName);
                SaveFolderTextBlock.Text = directory;
            }
        }
    }

    public class CodeVersion
    {
        public string VersionName { get; set; }
        public string Content { get; set; }
    }
}
