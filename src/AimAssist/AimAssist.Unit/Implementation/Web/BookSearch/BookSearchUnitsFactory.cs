using AimAssist.Units.Core;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Implementation.Web.BookSearch
{
    public class BookSearchUnitsFactory : IUnitsFacotry
    {
        public IMode TargetMode => BookSearchMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits()
        {
            yield return new Unit(TargetMode, "Book Search", new BookSearchSetting());
        }
    }
}
