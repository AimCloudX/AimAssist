using AimAssist.Units.Core.Mode;
using System.Windows.Media.Imaging;

namespace AimAssist.Units.Core.Units
{
    public class Unit : IUnit
    {
        public Unit(IMode mode, string name, IUnitContent content) : this(mode, name, string.Empty, content)
        {
        }
        public Unit(IMode mode, string name, string description, IUnitContent content) : this(mode, name, description, new BitmapImage(), content)
        {
        }
        public Unit(IMode mode, string name, string description, BitmapImage icon, IUnitContent content) : this(mode, name, description, string.Empty, icon, content)
        {
        }
        public Unit(IMode mode, string name, string description, string category, BitmapImage icon, IUnitContent content)
        {
            Mode = mode;
            Name = name;
            Description = description;
            Category = category;
            Icon = icon;
            Content = content;
        }

        public IMode Mode { get; }
        public string Name { get; }
        public string Description { get; }
        public string Category { get; }
        public BitmapImage Icon { get; }
        public IUnitContent Content { get; }

        public override bool Equals(object? obj)
        {
            if(obj is Unit unit)
            {
                return Equals(unit);
            }

            return base.Equals(obj);
        }

        public bool Equals(Unit unit)
        {
            return this.Name == unit.Name;
        }
    }


    public interface IUnit
    {
        IMode Mode { get; }
        string Name { get; }
        string Description { get; }
        string Category { get; }
        BitmapImage Icon { get; }
        IUnitContent Content { get; }
    }
}
