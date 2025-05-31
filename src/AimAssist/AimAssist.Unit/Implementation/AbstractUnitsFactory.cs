using AimAssist.Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AimAssist.Units.Implementation
{
    public abstract class AbstractUnitsFactory
    {
        protected readonly string FactoryName;
        protected readonly int Priority;

        protected AbstractUnitsFactory(string factoryName, int priority = 0)
        {
            FactoryName = factoryName ?? throw new ArgumentNullException(nameof(factoryName));
            Priority = priority;
        }

        public abstract IEnumerable<IUnit> CreateUnits();
        
        public virtual bool CanCreateUnits()
        {
            return true;
        }

        public virtual void Initialize()
        {
        }

        public virtual void Dispose()
        {
        }

        public string GetFactoryName() => FactoryName;
        public int GetPriority() => Priority;
    }

    public interface IUnitsFactoryManager
    {
        void RegisterFactory(AbstractUnitsFactory factory);
        void UnregisterFactory(string factoryName);
        IEnumerable<IUnit> GetAllUnits();
        IEnumerable<IUnit> GetUnitsFromFactory(string factoryName);
        IEnumerable<string> GetFactoryNames();
        void InitializeAllFactories();
        void DisposeAllFactories();
    }

    public class UnitsFactoryManager : IUnitsFactoryManager
    {
        private readonly Dictionary<string, AbstractUnitsFactory> _factories = new();
        private readonly object _lock = new();

        public void RegisterFactory(AbstractUnitsFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            lock (_lock)
            {
                if (_factories.ContainsKey(factory.GetFactoryName()))
                {
                    throw new InvalidOperationException($"Factory '{factory.GetFactoryName()}' is already registered");
                }

                _factories[factory.GetFactoryName()] = factory;
                factory.Initialize();
            }
        }

        public void UnregisterFactory(string factoryName)
        {
            if (string.IsNullOrEmpty(factoryName)) throw new ArgumentNullException(nameof(factoryName));

            lock (_lock)
            {
                if (_factories.TryGetValue(factoryName, out var factory))
                {
                    factory.Dispose();
                    _factories.Remove(factoryName);
                }
            }
        }

        public IEnumerable<IUnit> GetAllUnits()
        {
            var allUnits = new List<IUnit>();

            lock (_lock)
            {
                var sortedFactories = _factories.Values
                    .OrderByDescending(f => f.GetPriority())
                    .ThenBy(f => f.GetFactoryName());

                foreach (var factory in sortedFactories)
                {
                    if (factory.CanCreateUnits())
                    {
                        try
                        {
                            allUnits.AddRange(factory.CreateUnits());
                        }
                        catch (Exception ex)
                        {
                            // Log exception but continue with other factories
                            System.Diagnostics.Debug.WriteLine($"Error in factory '{factory.GetFactoryName()}': {ex.Message}");
                        }
                    }
                }
            }

            return allUnits;
        }

        public IEnumerable<IUnit> GetUnitsFromFactory(string factoryName)
        {
            if (string.IsNullOrEmpty(factoryName)) throw new ArgumentNullException(nameof(factoryName));

            lock (_lock)
            {
                if (_factories.TryGetValue(factoryName, out var factory) && factory.CanCreateUnits())
                {
                    return factory.CreateUnits();
                }
            }

            return Enumerable.Empty<IUnit>();
        }

        public IEnumerable<string> GetFactoryNames()
        {
            lock (_lock)
            {
                return _factories.Keys.ToList();
            }
        }

        public void InitializeAllFactories()
        {
            lock (_lock)
            {
                foreach (var factory in _factories.Values)
                {
                    try
                    {
                        factory.Initialize();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error initializing factory '{factory.GetFactoryName()}': {ex.Message}");
                    }
                }
            }
        }

        public void DisposeAllFactories()
        {
            lock (_lock)
            {
                foreach (var factory in _factories.Values)
                {
                    try
                    {
                        factory.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error disposing factory '{factory.GetFactoryName()}': {ex.Message}");
                    }
                }
                _factories.Clear();
            }
        }
    }
}
