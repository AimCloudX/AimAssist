namespace AimAssist.Core.Units;

public interface ISupportUnitsFactory
{
    IEnumerable<ISupportUnit> GetSupportUnits();
}