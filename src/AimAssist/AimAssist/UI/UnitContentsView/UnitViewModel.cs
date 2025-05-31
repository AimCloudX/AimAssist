using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using AimAssist.Core.Units;
using AimAssist.Units.Core.Units;
using Common.UI.WebUI;

namespace AimAssist.UI.UnitContentsView
{
    public class UnitViewModel : INotifyPropertyChanged
    {
        public UnitViewModel(IUnit content) : this(content.Mode.Icon, content)
        {
        }

        public UnitViewModel(UrlUnit urlUnit, System.Windows.Controls.Control control)
        {
            Task.Run(async () =>
                {
                    try
                    {
                        var icon = await FaviconFetcher.GetUrlIconAsync(urlUnit.Description);
                        control.Dispatcher.Invoke(() =>
                        {
                            var image = new System.Windows.Controls.Image { Source = icon };
                            this.Icon = image;
                        });
                    }
                    catch (Exception ex)
                    {
                        // エラーハンドリング
                    }
                });

            Content = urlUnit;
        }

        public UnitViewModel(DependencyObject icon, IUnit content)
        {
            Icon = icon;
            Content = content;
        }

        public IMode Mode =>Content.Mode;
        public string Name =>Content.Name;
        public string Description => Content.Description;
        public string Category => Content.Category;
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
        public IUnit Content { get; }

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
