using System.ComponentModel;

namespace AimAssist.Services.BookSearch
{
    public class SearchParameter : INotifyPropertyChanged
    {
        private SearchType _searchType = SearchType.Keyword;
        private string _searchValue = "";

        public SearchType SearchType
        {
            get => _searchType;
            set
            {
                _searchType = value;
                OnPropertyChanged(nameof(SearchType));
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public string SearchValue
        {
            get => _searchValue;
            set
            {
                _searchValue = value;
                OnPropertyChanged(nameof(SearchValue));
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public string SearchText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SearchValue))
                    return "";

                return SearchType == SearchType.ISBN ? $"isbn:{SearchValue}" : SearchValue;
            }
        }

        public string GetInputText()
        {
            return SearchType == SearchType.ISBN ? $"isbn:{SearchValue}" : SearchValue;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
