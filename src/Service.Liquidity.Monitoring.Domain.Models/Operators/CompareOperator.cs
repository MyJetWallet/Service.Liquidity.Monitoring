using System;

namespace Service.Liquidity.Monitoring.Domain.Models.Operators
{
    public struct CompareOperator
    {
        private readonly CompareOperatorType _type;

        public CompareOperator(CompareOperatorType type)
        {
            _type = type;
        }

        public bool Compare(decimal value, decimal targetValue)
        {
            switch (_type)
            {
                case CompareOperatorType.Bigger: return value > targetValue;
                case CompareOperatorType.Less: return value < targetValue;
                case CompareOperatorType.Equal: return value == targetValue;
                default: throw new NotSupportedException($"{_type.ToString()}");
            }
        }
    }
}