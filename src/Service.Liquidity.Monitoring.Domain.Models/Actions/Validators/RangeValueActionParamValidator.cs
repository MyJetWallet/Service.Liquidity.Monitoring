using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.Actions.Validators;

[DataContract]
public class RangeValueActionParamValidator : IActionParamValidator
{
    [DataMember(Order = 1)] public decimal MinValue { get; set; }
    [DataMember(Order = 2)] public decimal MaxValue { get; set; }

    public RangeValueActionParamValidator(decimal minValue, decimal maxValue)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }

    public bool IsValid(string value, out string message)
    {
        message = "";

        if (!decimal.TryParse(value, out var parsed) ||
            parsed < MinValue ||
            parsed > MaxValue)
        {
            message = $"{value} is not in range {MinValue} - {MaxValue}";
            return false;
        }

        return true;
    }
}