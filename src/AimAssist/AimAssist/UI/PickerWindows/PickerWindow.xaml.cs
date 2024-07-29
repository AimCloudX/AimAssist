using AimAssist.Service;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.KeyHelp;
using AimAssist.Units.Implementation.Snippets;
using Common.Commands.Shortcus;
using Common.UI;
using Common.UI.Editor;
using Library.Options;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace AimAssist.UI.PickerWindows
{
    public partial class PickerWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<UnitViewModel> UnitLists { get; } = new ObservableCollection<UnitViewModel>();

        public IMode Mode { get; set; }

        public string SnippetText { get; set; } = string.Empty;

        public KeySequence KeySequence { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private bool Filter(object obj)
        {
            var filterText = this.FilterTextBox.Text;

            if (string.IsNullOrEmpty(filterText))
            {
                return true;
            }

            var combo = obj as UnitViewModel;
            if (combo != null)
            {

                if (!combo.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsClosing { get; set; }

        DispatcherTimer? typingTimer;
        private string beforeText = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PickerWindow(string processName)
        {
            this.InitializeComponent();
            this.processName = processName;

            var editor = EditorCash.Editor;
            if(editor != null)
            {
                this.MainContent.Content = editor;
                editor.SetOption(EditorOptionService.Option);
            }
            else
            {
                var monacoEditor = new MonacoEditor();
                this.MainContent.Content = monacoEditor;
                EditorCash.Editor = monacoEditor;
                monacoEditor.SetOption(EditorOptionService.Option);
            }

            this.DataContext = this;

            this.Mode = SnippetMode.Instance;
            this.UpdateCandidate();
            this.FilterTextBox.Focus();

            this.ComboListBox.SelectedIndex = 0;
            RegisterSnippets();
        }

        public async void UpdateCandidate()
        {
            UnitLists.Clear();
            var units = UnitsService.Instnace.CreateUnits(this.Mode);

            foreach (var unit in units)
            {
                UnitLists.Add(new UnitViewModel(unit));
            }

            var keyUnits = UnitsService.Instnace.CreateUnits(KeyHelpMode.Instance);
            foreach (var unit in keyUnits)
            {
                if (unit is KeyHelpUnit keyHelpUnit)
                {
                    if(keyHelpUnit.Category != processName)
                    {
                        continue;
                    }
                }

                UnitLists.Add(new UnitViewModel(unit));
            }

            if (System.Windows.Clipboard.ContainsText())
            {
                UnitLists.Add(new UnitViewModel(new SnippetUnit("Clipboard", System.Windows.Clipboard.GetText())));
            }
        }

        private void HandleTypingTimerTimeout(object sender, EventArgs e)
        {
            if (this.typingTimer != null)
            {
                this.typingTimer.Tick -= this.HandleTypingTimerTimeout;
            }

            this.typingTimer = null;
            if (this.beforeText.Equals(this.FilterTextBox.Text))
            {
                return;
            }

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.ComboListBox.Items);
            view.Filter = this.Filter;
            this.typingTimer = null;
            this.beforeText = this.FilterTextBox.Text;
            this.OnPropertyChanged(nameof(this.UnitLists));
            this.ComboListBox.SelectedIndex = 0;
        }

        private async void TextBox_OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (this.ComboListBox.SelectedItem is UnitViewModel unit)
                {
                    if (unit.Content is SnippetUnit model)
                    {
                        this.SnippetText = await EditorCash.Editor.GetText();
                        this.CloseWindow();
                    }

                    if (unit.Content is SnippetModelUnit snippetModel)
                    {
                        this.SnippetText = await EditorCash.Editor.GetText();
                        this.CloseWindow();
                    }

                    if (unit.Content is KeyHelpUnit keyHelpUnit)
                    {
                        this.KeySequence = keyHelpUnit.KeyItem.Sequence;
                        this.CloseWindow();
                    }
                }

            }

            else if (e.Key == Key.Escape)
            {
                this.CloseWindow();
            }

            if (e.Key == Key.Up)
            {
                var index = this.ComboListBox.SelectedIndex;
                if (index <= 0) return;
                this.ComboListBox.SelectedIndex = index - 1;
            }

            if (e.Key == Key.Down)
            {
                var index = this.ComboListBox.SelectedIndex;
                if (index >= this.ComboListBox.Items.Count) return;
                this.ComboListBox.SelectedIndex = index + 1;
            }

            this.ComboListBox.ScrollIntoView(this.ComboListBox.SelectedItem);
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.typingTimer == null)
            {
                this.typingTimer = new DispatcherTimer
                {
                    Interval = default,
                    IsEnabled = false,
                    Tag = null
                };

                this.typingTimer.Interval = TimeSpan.FromMilliseconds(100);

                this.typingTimer.Tick += this.HandleTypingTimerTimeout;
            }

            this.typingTimer.Stop(); // Resets the timer
            this.typingTimer.Start();
        }

        private void CloseWindow()
        {
            if (this.IsClosing)
            {
                return;
            }

            this.IsClosing = true;
            EditorCash.Editor = (MonacoEditor)this.MainContent.Content;
            this.Close();
        }

        public void FocusContent()
        {
            if (this.MainContent.Content is IFocasable focusable)
            {
                focusable.Focus();
            }
        }


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.IsClosing = true;
        }

        private void Window_LostFocus(object sender, EventArgs e)
        {
            //this.CloseWindow();
        }

        private void ComboListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ComboListBox.SelectedItem is UnitViewModel unit && unit.Content is SnippetUnit model)
            {
                EditorCash.Editor.SetTextAsync(model.Code);
            }

            if (this.ComboListBox.SelectedItem is UnitViewModel unitViewModel
                && unitViewModel.Content is SnippetModelUnit snippetModel)
            {
                EditorCash.Editor.SetTextAsync(snippetModel.Code);
            }
        }

        private readonly KeySequenceManager _keySequenceManager = new KeySequenceManager();
        private readonly string processName;

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_keySequenceManager.HandleKeyPress(e.Key, Keyboard.Modifiers, this))
            {
                e.Handled = true;
            }
        }

        private void RegisterSnippets()
        {
            var snippetVariables = new List<EditorSnippet>
{
    //new Snippet { Label = "TM_SELECTED_TEXT", InsertText = "${TM_SELECTED_TEXT}", Documentation = "The currently selected text or the empty string" },
    //new Snippet { Label = "TM_CURRENT_LINE", InsertText = "${TM_CURRENT_LINE}", Documentation = "The contents of the current line" },
    //new Snippet { Label = "TM_CURRENT_WORD", InsertText = "${TM_CURRENT_WORD}", Documentation = "The contents of the word under cursor or the empty string" },
    //new Snippet { Label = "TM_LINE_INDEX", InsertText = "${TM_LINE_INDEX}", Documentation = "The zero-index based line number" },
    //new Snippet { Label = "TM_LINE_NUMBER", InsertText = "${TM_LINE_NUMBER}", Documentation = "The one-index based line number" },
    //new Snippet { Label = "TM_FILENAME", InsertText = "${TM_FILENAME}", Documentation = "The filename of the current document" },
    //new Snippet { Label = "TM_FILENAME_BASE", InsertText = "${TM_FILENAME_BASE}", Documentation = "The filename of the current document without its extensions" },
    //new Snippet { Label = "TM_DIRECTORY", InsertText = "${TM_DIRECTORY}", Documentation = "The directory of the current document" },
    //new Snippet { Label = "TM_FILEPATH", InsertText = "${TM_FILEPATH}", Documentation = "The full file path of the current document" },
    //new Snippet { Label = "WORKSPACE_NAME", InsertText = "${WORKSPACE_NAME}", Documentation = "The name of the opened workspace or folder" },
    
    // Date and Time variables
    //new Snippet { Label = "CURRENT_YEAR", InsertText = "${CURRENT_YEAR}", Documentation = "The current year" },
    //new Snippet { Label = "CURRENT_YEAR_SHORT", InsertText = "${CURRENT_YEAR_SHORT}", Documentation = "The current year's last two digits" },
    //new Snippet { Label = "CURRENT_MONTH", InsertText = "${CURRENT_MONTH}", Documentation = "The month as two digits (example '02')" },
    //new Snippet { Label = "CURRENT_MONTH_NAME", InsertText = "${CURRENT_MONTH_NAME}", Documentation = "The full name of the month (example 'July')" },
    //new Snippet { Label = "CURRENT_MONTH_NAME_SHORT", InsertText = "${CURRENT_MONTH_NAME_SHORT}", Documentation = "The short name of the month (example 'Jul')" },
    //new Snippet { Label = "CURRENT_DATE", InsertText = "${CURRENT_DATE}", Documentation = "The day of the month" },
    //new Snippet { Label = "CURRENT_DAY_NAME", InsertText = "${CURRENT_DAY_NAME}", Documentation = "The name of day (example 'Monday')" },
    //new Snippet { Label = "CURRENT_DAY_NAME_SHORT", InsertText = "${CURRENT_DAY_NAME_SHORT}", Documentation = "The short name of the day (example 'Mon')" },
    //new Snippet { Label = "CURRENT_HOUR", InsertText = "${CURRENT_HOUR}", Documentation = "The current hour in 24-hour clock format" },
    //new Snippet { Label = "CURRENT_MINUTE", InsertText = "${CURRENT_MINUTE}", Documentation = "The current minute" },
    //new Snippet { Label = "CURRENT_SECOND", InsertText = "${CURRENT_SECOND}", Documentation = "The current second" },
    //new Snippet { Label = "CURRENT_SECONDS_UNIX", InsertText = "${CURRENT_SECONDS_UNIX}", Documentation = "The number of seconds since the Unix epoch" }

    new EditorSnippet { Label = "CURRENT_YEAR", InsertText = "\\\\${CURRENT_YEAR}", Documentation = "The current year" },
    new EditorSnippet { Label = "CURRENT_MONTH", InsertText = "\\\\${CURRENT_MONTH}", Documentation = "The month as two digits (example '02')" },
    new EditorSnippet { Label = "CURRENT_DATE", InsertText = "\\\\${CURRENT_DATE}", Documentation = "The day of the month" },
    new EditorSnippet { Label = "CURRENT_HOUR", InsertText = "\\\\${CURRENT_HOUR}", Documentation = "The current hour in 24-hour clock format" },
    new EditorSnippet { Label = "CURRENT_MINUTE", InsertText = "\\\\${CURRENT_MINUTE}", Documentation = "The current minute" },
    new EditorSnippet { Label = "CURRENT_SECOND", InsertText = "\\\\${CURRENT_SECOND}", Documentation = "The current second" },
};

            //EditorCash.Editor.RegisterSnippets(snippetVariables);
        }

    }
}