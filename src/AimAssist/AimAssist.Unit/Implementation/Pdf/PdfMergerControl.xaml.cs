using Microsoft.Win32;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AimAssist.Core.Attributes;

namespace AimAssist.Units.Implementation.Pdf
{
    [AutoDataTemplate(typeof(PdfMergeUnit))]
    public partial class PdfMergerControl : UserControl
    {
        public ObservableCollection<PdfFile> PdfFiles { get; set; }
        private int draggedItemIndex = -1;
        private int dropTargetIndex = -1;

        public PdfMergerControl()
        {
            InitializeComponent();
            PdfFiles = new ObservableCollection<PdfFile>();
            FileListBox.ItemsSource = PdfFiles;
            DataContext = this;

            AllowDrop = true;
            FileListBox.AllowDrop = true;
            FileListBox.DragEnter += FileListBox_DragEnter;
            FileListBox.DragOver += FileListBox_DragOver;
            FileListBox.Drop += FileListBox_Drop;
            FileListBox.PreviewMouseLeftButtonDown += FileListBox_PreviewMouseLeftButtonDown;
            FileListBox.PreviewMouseMove += FileListBox_PreviewMouseMove;
        }

        private void FileListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }
        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem != null)
            {
                DragDrop.DoDragDrop(listBox, listBox.SelectedItem, DragDropEffects.Move);
            }
        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(string)))
            {
                var droppedData = e.Data.GetData(typeof(string)) as string;
                var listBox = sender as ListBox;

                if (listBox?.ItemsSource is ObservableCollection<string> items && droppedData != null)
                {
                    int oldIndex = items.IndexOf(droppedData);
                    Point dropPosition = e.GetPosition(listBox);
                    var targetItem = listBox.InputHitTest(dropPosition) as FrameworkElement;
                    if (targetItem?.DataContext is string targetData)
                    {
                        int newIndex = items.IndexOf(targetData);
                        if (oldIndex != newIndex)
                        {
                            items.Move(oldIndex, newIndex);
                        }
                    }
                }
            }
        }
        private void FileListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void FileListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(PdfFile)))
            {
                var droppedData = e.Data.GetData(typeof(PdfFile)) as PdfFile;
                var listBox = sender as ListBox;

                if (listBox?.ItemsSource is ObservableCollection<PdfFile> items && droppedData != null)
                {
                    int oldIndex = items.IndexOf(droppedData);
                    Point dropPosition = e.GetPosition(listBox);
                    var targetItem = listBox.InputHitTest(dropPosition) as FrameworkElement;
                    if (targetItem?.DataContext is PdfFile targetData)
                    {
                        int newIndex = items.IndexOf(targetData);
                        if (oldIndex != newIndex)
                        {
                            items.Move(oldIndex, newIndex);
                        }
                    }
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[]? files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
                if (files != null)
                {
                    AddPdfFiles(files.Select(x=>new PdfFile(x)).ToArray());
                }
            }
        }

        private void FileListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            draggedItemIndex = FileListBox.SelectedIndex;
        }

        private void FileListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && draggedItemIndex != -1)
            {
                DragDrop.DoDragDrop(FileListBox, PdfFiles[draggedItemIndex], DragDropEffects.Move);
            }
        }

        private void AddPdfFiles(PdfFile[] paths)
        {
            foreach (var file in paths)
            {
                if (System.IO.Path.GetExtension(file.FilePath).ToLower() == ".pdf" &&
                    !PdfFiles.Any(x=>x.FilePath.Equals(file.FilePath, StringComparison.OrdinalIgnoreCase)))
                {
                    PdfFiles.Add(file);
                }
            }
        }

        private void AddPdfButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "PDFファイル (*.pdf)|*.pdf"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                AddPdfFiles(openFileDialog.FileNames.Select(x => new PdfFile(x)).ToArray());
            }
        }

        private void RemovePdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileListBox.SelectedItem is PdfFile pdfFile)
            {
                PdfFiles.Remove(pdfFile);
            }
        }

        private void MergePdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (PdfFiles.Count < 2)
            {
                MessageBox.Show("少なくとも2つのPDFファイルを選択してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDFファイル (*.pdf)|*.pdf",
                FileName = "MergedDocument.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (PdfDocument outputDocument = new PdfDocument())
                    {
                        foreach (var pdfPath in PdfFiles)
                        {
                            using (PdfDocument inputDocument = PdfReader.Open(pdfPath.FilePath, PdfDocumentOpenMode.Import))
                            {
                                for (int i = 0; i < inputDocument.PageCount; i++)
                                {
                                    outputDocument.AddPage(inputDocument.Pages[i]);
                                }
                            }
                        }

                        outputDocument.Save(saveFileDialog.FileName);
                    }

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = saveFileDialog.FileName,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"PDFの結合中にエラーが発生しました：{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            PdfFiles.Clear();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (FileListBox.SelectedItem is PdfFile pdfFile)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = pdfFile.FilePath,
                    UseShellExecute = true
                });
            }
        }
    }

    public class PdfFile
    {
        public PdfFile(string filePath)
        {
            FilePath = filePath;
            FileName  = System.IO.Path.GetFileName(filePath);
        }

        public string FilePath { get; }
        public string FileName { get; }
    }
}
