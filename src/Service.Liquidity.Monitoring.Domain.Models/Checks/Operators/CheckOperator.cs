using System;

namespace Service.Liquidity.Monitoring.Domain.Models.Checks.Operators
{
    public class CheckOperator
    {
        private readonly CheckOperatorType _type;

        public CheckOperator(CheckOperatorType type)
        {
            _type = type;
        }

        public bool Compare(decimal value, decimal targetValue)
        {
            switch (_type)
            {
                case CheckOperatorType.Bigger: return value > targetValue;
                case CheckOperatorType.Less: return value < targetValue;
                case CheckOperatorType.Equal: return value == targetValue;
                default: throw new NotSupportedException($"{_type.ToString()}");
            }
        }
    }
}