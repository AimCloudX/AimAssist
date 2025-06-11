namespace AimAssist.Core.Units;

public interface ISupportUnit :IUnit
{
    IMode SupportTarget { get; }
}