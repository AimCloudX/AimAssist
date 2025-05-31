using AimAssist.Commands;
using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Service;
using AimAssist.UI.PickerWindows;
using AimAssist.UI.UnitContentsView;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.Caluculation;
using AimAssist.Units.Implementation.KeyHelp;
using AimAssist.Units.Implementation.Snippets;
using Common.UI.Editor;
using Mathos.Parser;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Common.UI.Commands.Shortcus;
using UnitViewModel = AimAssist.UI.UnitContentsView.UnitViewModel;

namespace AimAssist.ViewModels
{
    public class PickerWindowViewModel : INotifyPropertyChanged
    {
        private readonly IUnitsService _unitsService;
        private readonly IEditorOptionService _editorOptionService;
        private readonly string _processName;
        private DispatcherTimer? _filterTimer;

        private IMode _mode;
        private int _selectedIndex;
        private string _filterText = string.Empty;
        private UnitViewModel? _selectedUnit;

        public ObservableCollection<UnitViewModel> UnitLists { get; } = new();

        public IMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                OnPropertyChanged();
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                OnPropertyChanged();
                OnFilterTextChanged();
            }
        }

        public UnitViewModel? SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                _selectedUnit = value;
                OnPropertyChanged();
                OnSelectedUnitChanged();
            }
        }

        public string SnippetText { get; set; } = string.Empty;
        public KeySequence KeySequence { get; set; }
        public bool IsClosing { get; set; }

        public ICommand NavigateUpCommand { get; }
        public ICommand NavigateDownCommand { get; }
        public ICommand ExecuteCommand { get; }
        public ICommand CloseCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public PickerWindowViewModel(string processName, IUnitsService unitsService, IEditorOptionService editorOptionService)
        {
            _processName = processName;
            _unitsService = unitsService;
            _editorOptionService = editorOptionService;
            _mode = SnippetMode.Instance;

            NavigateUpCommand = new RelayCommand(NavigateUp);
            NavigateDownCommand = new RelayCommand(NavigateDown);
            ExecuteCommand = new RelayCommand(async () => await ExecuteSelectedUnit());
            CloseCommand = new RelayCommand(() => IsClosing = true);

            UpdateCandidate();
        }

        private void OnFilterTextChanged()
        {
            _filterTimer?.Stop();
            _filterTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _filterTimer.Tick += (s, e) =>
            {
                _filterTimer.Stop();
                _filterTimer = null;

                if (!string.IsNullOrEmpty(_filterText) && _filterText.StartsWith('='))
                {
                    HandleCalculationMode();
                }
                else
                {
                    HandleSnippetMode();
                }
            };
            _filterTimer.Start();
        }

        private void HandleCalculationMode()
        {
            Mode = CalcMode.Instance;

            try
            {
                var expression = _filterText.Remove(0, 1);
                var parser = new MathParser();
                var result = parser.Parse(expression);
                var unitViewModel = new UnitViewModel(new SnippetUnit(result.ToString(), result.ToString()));

                UnitLists.Clear();
                UnitLists.Add(unitViewModel);
                SelectedIndex = 0;
            }
            catch
            {
                // 計算エラーは無視
            }
        }

        private void HandleSnippetMode()
        {
            if (Mode != SnippetMode.Instance)
            {
                Mode = SnippetMode.Instance;
                UpdateCandidate();
            }

            var view = CollectionViewSource.GetDefaultView(UnitLists);
            view.Filter = FilterUnits;
            SelectedIndex = 0;
        }

        private bool FilterUnits(object obj)
        {
            if (string.IsNullOrEmpty(_filterText))
                return true;

            if (obj is UnitViewModel unit)
            {
                return unit.Name.Contains(_filterText, StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        private void OnSelectedUnitChanged()
        {
            if (_selectedUnit?.Content is SnippetUnit snippetUnit)
            {
                EditorCache.Editor?.SetTextAsync(snippetUnit.Code);
            }
            else if (_selectedUnit?.Content is SnippetModelUnit snippetModelUnit)
            {
                EditorCache.Editor?.SetTextAsync(snippetModelUnit.Code);
            }
        }

        public void UpdateCandidate()
        {
            UnitLists.Clear();
            var units = _unitsService.CreateUnits(Mode);

            foreach (var unit in units)
            {
                UnitLists.Add(new UnitViewModel(unit));
            }

            if (System.Windows.Clipboard.ContainsText())
            {
                UnitLists.Add(new UnitViewModel(new SnippetUnit("Clipboard", System.Windows.Clipboard.GetText())));
            }

            var keyUnits = _unitsService.CreateUnits(KeyHelpMode.Instance);
            foreach (var unit in keyUnits)
            {
                if (unit is KeyHelpUnit keyHelpUnit && keyHelpUnit.Category == _processName)
                {
                    UnitLists.Add(new UnitViewModel(unit));
                }
            }
        }

        private void NavigateUp()
        {
            if (SelectedIndex > 0)
            {
                SelectedIndex--;
            }
        }

        private void NavigateDown()
        {
            if (SelectedIndex < UnitLists.Count - 1)
            {
                SelectedIndex++;
            }
        }

        private async Task ExecuteSelectedUnit()
        {
            if (SelectedUnit?.Content is SnippetUnit)
            {
                SnippetText = await EditorCache.Editor?.GetText() ?? string.Empty;
                IsClosing = true;
            }
            else if (SelectedUnit?.Content is SnippetModelUnit)
            {
                SnippetText = await EditorCache.Editor?.GetText() ?? string.Empty;
                IsClosing = true;
            }
            else if (SelectedUnit?.Content is KeyHelpUnit keyHelpUnit)
            {
                KeySequence = keyHelpUnit.KeyItem.Sequence;
                IsClosing = true;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
