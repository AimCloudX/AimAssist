using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using AimAssist.Commands;
using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.Caluculation;
using AimAssist.Units.Implementation.KeyHelp;
using AimAssist.Units.Implementation.Snippets;
using Common.UI.Commands.Shortcus;
using Mathos.Parser;
using UnitViewModel = AimAssist.UI.UnitContentsView.UnitViewModel;

namespace AimAssist.UI.PickerWindows
{
    public class PickerWindowViewModel : INotifyPropertyChanged
    {
        private readonly IUnitsService _unitsService;
        private readonly IEditorOptionService _editorOptionService;
        private readonly string _processName;
        private DispatcherTimer? _filterTimer;

        private IMode _mode = null!;
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
                
                // SelectedIndexが変更されたときにSelectedUnitも更新
                if (_selectedIndex >= 0 && _selectedIndex < UnitLists.Count)
                {
                    var view = CollectionViewSource.GetDefaultView(UnitLists);
                    var filteredItems = view.Cast<UnitViewModel>().ToList();
                    if (_selectedIndex < filteredItems.Count)
                    {
                        SelectedUnit = filteredItems[_selectedIndex];
                    }
                }
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
        public KeySequence KeySequence { get; set; } = null!;
        
        private bool _isClosing;
        public bool IsClosing 
        { 
            get => _isClosing;
            set
            {
                _isClosing = value;
                OnPropertyChanged();
            }
        }

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
            // モード切り替え時にフィルタをクリア
            if (Mode != CalcMode.Instance)
            {
                Mode = CalcMode.Instance;
                var view = CollectionViewSource.GetDefaultView(UnitLists);
                view.Filter = null; // フィルタをクリア
            }

            try
            {
                var expression = _filterText.Remove(0, 1);
                if (string.IsNullOrWhiteSpace(expression))
                {
                    UnitLists.Clear();
                    return;
                }

                var parser = new MathParser();
                var result = parser.Parse(expression);
                var calcUnit = new CalcUnit(expression, result.ToString());
                var unitViewModel = UnitViewModel.Instance(calcUnit);

                UnitLists.Clear();
                UnitLists.Add(unitViewModel);
                SelectedIndex = 0;
                SelectedUnit = unitViewModel; // 明示的に選択状態を設定
            }
            catch (Exception ex)
            {
                // 計算エラーの場合、エラーメッセージを表示
                var errorUnit = new CalcUnit(_filterText.Remove(0, 1), $"エラー: {ex.Message}");
                var unitViewModel = UnitViewModel.Instance(errorUnit);
                
                UnitLists.Clear();
                UnitLists.Add(unitViewModel);
                SelectedIndex = 0;
                SelectedUnit = unitViewModel; // 明示的に選択状態を設定
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
            view.Refresh(); // フィルタの再適用を強制
            
            // フィルタ適用後に最初の項目を選択
            if (UnitLists.Count > 0)
            {
                SelectedIndex = 0;
                var filteredItems = view.Cast<UnitViewModel>().ToList();
                if (filteredItems.Count > 0)
                {
                    SelectedUnit = filteredItems[0];
                }
            }
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
                // Fire-and-forget is intentional for UI responsiveness
                _ = EditorCache.Editor?.SetTextAsync(snippetUnit.Code);
            }
            else if (_selectedUnit?.Content is SnippetModelUnit snippetModelUnit)
            {
                // Fire-and-forget is intentional for UI responsiveness
                _ = EditorCache.Editor?.SetTextAsync(snippetModelUnit.Code);
            }
            else if (_selectedUnit?.Content is CalcUnit calcUnit)
            {
                // Fire-and-forget is intentional for UI responsiveness
                _ = EditorCache.Editor?.SetTextAsync(calcUnit.Result);
            }
        }

        public void UpdateCandidate()
        {
            UnitLists.Clear();
            
            // フィルタをクリア
            var view = CollectionViewSource.GetDefaultView(UnitLists);
            view.Filter = null;
            
            var units = _unitsService.CreateUnits(Mode);

            foreach (var unit in units)
            {
                UnitLists.Add(UnitViewModel.Instance(unit));
            }

            if (System.Windows.Clipboard.ContainsText())
            {
                UnitLists.Add(UnitViewModel.Instance(new SnippetUnit("Clipboard", System.Windows.Clipboard.GetText())));
            }

            var keyUnits = _unitsService.CreateUnits(KeyHelpMode.Instance);
            foreach (var unit in keyUnits)
            {
                if (unit is KeyHelpUnit keyHelpUnit && keyHelpUnit.Category == _processName)
                {
                    UnitLists.Add(UnitViewModel.Instance(unit));
                }
            }
            
            // 最初の項目を選択
            if (UnitLists.Count > 0)
            {
                SelectedIndex = 0;
                SelectedUnit = UnitLists[0];
            }
        }

        private void NavigateUp()
        {
            var view = CollectionViewSource.GetDefaultView(UnitLists);
            var filteredItems = view.Cast<UnitViewModel>().ToList();
            
            if (SelectedIndex > 0 && filteredItems.Count > 0)
            {
                SelectedIndex--;
            }
        }

        private void NavigateDown()
        {
            var view = CollectionViewSource.GetDefaultView(UnitLists);
            var filteredItems = view.Cast<UnitViewModel>().ToList();
            
            if (SelectedIndex < filteredItems.Count - 1)
            {
                SelectedIndex++;
            }
        }

        private async Task ExecuteSelectedUnit()
        {
            if (SelectedUnit?.Content is SnippetUnit)
            {
                var text = EditorCache.Editor?.GetText();
                SnippetText = (text != null ? await text : string.Empty) ?? string.Empty;
                IsClosing = true;
            }
            else if (SelectedUnit?.Content is SnippetModelUnit)
            {
                var text = EditorCache.Editor?.GetText();
                SnippetText = (text != null ? await text : string.Empty) ?? string.Empty;
                IsClosing = true;
            }
            else if (SelectedUnit?.Content is CalcUnit calcUnit)
            {
                SnippetText = calcUnit.Result;
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
