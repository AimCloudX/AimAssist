using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;

namespace AimAssist.Unit.Implementation.Web.BookSearch
{
    public class BookSearchUnitsFactory : IUnitsFacotry
    {
        public IPickerMode TargetMode => BookSearchMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter parameter)
        {
            yield return new BookSearchUnit();
        }
    }
}
