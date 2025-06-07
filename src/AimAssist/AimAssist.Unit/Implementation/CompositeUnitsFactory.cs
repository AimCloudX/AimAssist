using AimAssist.Core.Units;
using AimAssist.Units.Implementation.Factories;
using System;

namespace AimAssist.Units.Implementation
{
    public interface ICompositeUnitsFactory : IUnitsFactory
    {
        void RegisterSubFactory(AbstractUnitsFactory factory);
        void UnregisterSubFactory(string factoryName);
    }

    public class CompositeUnitsFactory : AbstractUnitsFactory, ICompositeUnitsFactory
    {
        private readonly IUnitsFactoryManager _factoryManager;

        public CompositeUnitsFactory(IUnitsFactoryManager factoryManager) 
            : base("Composite", priority: 100)
        {
            _factoryManager = factoryManager ?? throw new ArgumentNullException(nameof(factoryManager));
        }

        public override IEnumerable<IUnit> CreateUnits()
        {
            return _factoryManager.GetAllUnits();
        }

        public IEnumerable<IUnit> GetUnits()
        {
            return CreateUnits();
        }

        public void RegisterSubFactory(AbstractUnitsFactory factory)
        {
            _factoryManager.RegisterFactory(factory);
        }

        public void UnregisterSubFactory(string factoryName)
        {
            _factoryManager.UnregisterFactory(factoryName);
        }

        public override void Initialize()
        {
            _factoryManager.InitializeAllFactories();
        }

        public override void Dispose()
        {
            _factoryManager.DisposeAllFactories();
        }
    }
}
