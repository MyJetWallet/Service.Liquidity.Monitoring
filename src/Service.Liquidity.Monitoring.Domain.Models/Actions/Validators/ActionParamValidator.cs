using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.Actions.Validators;

[DataContract]
public class ActionParamValidator : IActionParamValidator
{
    [DataMember(Order = 1)] public ValidatorType Type { get; set; }
    [DataMember(Order = 2)] public Dictionary<string, string> ParamValuesByName { get; set; }

    public bool IsValid(string value, out string message)
    {
        var validator = GetValidator();

        return validator.IsValid(value, out message);
    }

    private IActionParamValidator GetValidator()
    {
        switch (Type)
        {
            case ValidatorType.Range:
            {
                return new RangeValueActionParamValidator(ParamValuesByName);
            }
            default: throw new Exception($"Not supported {Type.ToString()}");
        }
    }
}