using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.Actions.Validators;

[DataContract]
public class RangeValueActionParamValidator : IActionParamValidator
{
    [DataMember(Order = 1)] public ValidatorType Type { get; set; } = ValidatorType.Range;
    [DataMember(Order = 2)] public Dictionary<string, string> ParamValuesByName { get; set; } = new();

    [DataMember(Order = 3)]
    public decimal MaxValue
    {
        get
        {
            var strValue = ParamValuesByName[nameof(MaxValue)];

            return decimal.Parse(strValue);
        }
        set => ParamValuesByName[nameof(MaxValue)] = value.ToString(CultureInfo.InvariantCulture);
    }

    [DataMember(Order = 4)]
    public decimal MinValue
    {
        get
        {
            var strValue = ParamValuesByName[nameof(MinValue)];

            return decimal.Parse(strValue);
        }
        set => ParamValuesByName[nameof(MinValue)] = value.ToString(CultureInfo.InvariantCulture);
    }

    public RangeValueActionParamValidator(decimal minValue, decimal maxValue)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }

    public RangeValueActionParamValidator(Dictionary<string, string> paramValuesByName)
    {
        ParamValuesByName = paramValuesByName;
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