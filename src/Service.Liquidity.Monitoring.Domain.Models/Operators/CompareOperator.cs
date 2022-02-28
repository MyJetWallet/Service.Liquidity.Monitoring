using System;

namespace Service.Liquidity.Monitoring.Domain.Models.Operators
{
    public readonly struct CompareOperator
    {
        private readonly CompareOperatorType _type;

        public CompareOperator(CompareOperatorType type)
        {
            _type = type;
        }

        public bool Compare(decimal actualValue, decimal targetValue)
        {
            return _type switch
            {
                CompareOperatorType.Bigger => actualValue > targetValue,
                CompareOperatorType.Less => actualValue < targetValue,
                CompareOperatorType.Equal => actualValue == targetValue,
                _ => throw new NotSupportedException($"{_type.ToString()}")
            };
        }
    }
}