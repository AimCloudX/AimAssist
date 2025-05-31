using AimAssist.Core.Units;
using AimAssist.Units.Implementation.Caluculation;

namespace AimAssist.Units.Implementation.Caluculation
{
    public class CalcUnit : IUnit
    {
        public CalcUnit(string expression, string result)
        {
            Name = $"{expression} = {result}";
            Code = result;
            Category = "計算結果";
            Expression = expression;
            Result = result;
        }

        public string Name { get; }
        public string Code { get; }
        public string Category { get; }
        public string Expression { get; }
        public string Result { get; }
        public IMode Mode => CalcMode.Instance;
        public string Description => $"計算式: {Expression}";
    }
}
