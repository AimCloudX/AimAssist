namespace AimAssist.Core.Units;

public interface IFeaturesFactory
{
    IEnumerable<IFeature> GetFeatures();
}