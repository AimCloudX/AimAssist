namespace AimPicker.Unit.Core
{
    public interface IUnitPackage : IUnit
    {
        IEnumerable<IUnit> GetChildren(); 
    }
}
