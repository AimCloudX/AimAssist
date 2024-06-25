using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;

namespace AimAssist.Unit.Implementation.Web.BookSearch
{
    public class BookSearchUnitsFactory : IUnitsFacotry
    {
        public IMode TargetMode => BookSearchMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter parameter)
        {
            yield return new BookSearchUnit();
        }
    }
}
