using AimPicker.Unit.Core;

namespace AimPicker.Service
{
    internal class FactoryContainer
    {
        private IList<IUnitsFacotry> unitsFacotries = new List<IUnitsFacotry>();

        public FactoryContainer()
        {
        }

        public void RegisterFactory (IUnitsFacotry factory)
        {
            unitsFacotries.Add(factory);
        }
    }
}
