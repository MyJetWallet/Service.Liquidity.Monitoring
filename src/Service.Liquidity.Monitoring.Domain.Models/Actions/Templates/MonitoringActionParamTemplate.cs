using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Actions.Validators;

namespace Service.Liquidity.Monitoring.Domain.Models.Actions.Templates;

[DataContract]
public class MonitoringActionParamTemplate
{
    [DataMember(Order = 1)] public string Name { get; set; }
    [DataMember(Order = 2)] public string DisplayName { get; set; }
    [DataMember(Order = 3)] public string Value { get; set; }
    [DataMember(Order = 4)] public string DisplayValue { get; set; }
    [DataMember(Order = 5)] public MonitoringActionParamType Type { get; set; }

    [DataMember(Order = 6)] public ICollection<(string Value, string DisplayValue)> PossibleValues { get; set; } =
        new List<(string Value, string DisplayValue)>();

    [DataMember(Order = 7)] public ICollection<IActionParamValidator> Validators { get; set; } = new List<IActionParamValidator>();
    [DataMember(Order = 8)] public bool Readonly { get; set; }

    public bool Validate(out string message)
    {
        message = "";

        if (string.IsNullOrWhiteSpace(Value))
        {
            message = $"{DisplayName} can't be empty";
            return false;
        }
        
        var isValidType = Type switch
        {
            MonitoringActionParamType.Decimal => decimal.TryParse(Value, out _),
            MonitoringActionParamType.Int => int.TryParse(Value, out _),
            MonitoringActionParamType.String => true,
            _ => throw new NotSupportedException($"{nameof(Type)}")
        };

        if (!isValidType)
        {
            message = $"Invalid value {Value} for {DisplayName}. Must be {Type.ToString()}";
            return false;
        }

        foreach (var validator in Validators)
        {
            if (!validator.IsValid(Value, out var validatorMessage))
            {
                message = $"Invalid value {Value} for {DisplayName}. {validatorMessage}";
                return false;
            }
        }

        return true;
    }
}