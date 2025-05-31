using AimAssist.Core.Units;
using System.Windows.Media.Imaging;

namespace AimAssist.Units.Core.Units
{
    public class UnitViewModel 
    {
        public UnitViewModel(IUnit content) : this(new BitmapImage(), content)
        {
        }
        public UnitViewModel(BitmapImage icon, IUnit content)
        {
            Icon = icon;
            Content = content;
        }

        public IMode Mode =>Content.Mode;
        public string Name =>Content.Name;
        public string Description => Content.Description;
        public string Category => Content.Category;
        public BitmapImage Icon { get; }
        public IUnit Content { get; }

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
