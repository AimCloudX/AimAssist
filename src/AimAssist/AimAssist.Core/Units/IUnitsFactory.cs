namespace AimAssist.Core.Units
{
    public interface IUnitsFactory
    {
        IEnumerable<IUnit> GetUnits();
    }
}
