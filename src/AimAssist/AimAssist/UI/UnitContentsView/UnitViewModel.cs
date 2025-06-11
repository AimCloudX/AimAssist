using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using AimAssist.Core.Units;
using AimAssist.Units.Core.Units;
using AimAssist.Services.Markdown;
using Common.UI.WebUI;

namespace AimAssist.UI.UnitContentsView
{
    public class UnitViewModel : INotifyPropertyChanged
    {
        public static UnitViewModel Instance(IItem unit)
        {
            if (unit is UrlUnit urlUnit)
            {
                return new UnitViewModel(urlUnit);
            }
            
            return new UnitViewModel(unit.Mode.Icon,unit);
        }

        private UnitViewModel(UrlUnit urlUnit)
        {
            Content = urlUnit;
            InitializeIconAsync(urlUnit);
        }

        private async void InitializeIconAsync(UrlUnit urlUnit)
        {
            try
            {
                var icon = await FaviconFetcher.GetUrlIconAsync(urlUnit.Description).ConfigureAwait(true);
                var image = new System.Windows.Controls.Image { Source = icon };
                this.Icon = image;
            }
            catch (Exception ex)
            {
            }
        }

        private UnitViewModel(DependencyObject icon, IItem content)
        {
            Icon = icon;
            Content = content;
        }

        public IMode Mode => Content.Mode;
        public string Name => Content.Name;
        public string Description => Content.Description;
        public string Category => Content.Category;
        public string CategorySortKey => GetCategorySortKey();
        
        private string GetCategorySortKey()
        {
            if (string.IsNullOrEmpty(Content.Category))
            {
                return "zzz";
            }
            
            var order = CategoryOrderManager.GetCategoryOrder(Content.Category);
            if (order != int.MaxValue - 1)
            {
                return order.ToString("D3");
            }
            
            return Content.Category;
        }
        
        public DependencyObject Icon
        {
            get { return this.icon; }
            set {
                this.icon = value;
                this.OnPropertyChanged(nameof(Icon));
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private DependencyObject icon { get; set; }
        public IItem Content { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public override bool Equals(object? obj)
        {
            if(obj is UnitViewModel unit)
            {
                return Equals(unit);
            }

            return base.Equals(obj);
        }

        public bool Equals(UnitViewModel unit)
        {
            return this.Name == unit.Name;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}