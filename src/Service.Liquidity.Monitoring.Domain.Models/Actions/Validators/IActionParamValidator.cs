using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.Actions.Validators;

public interface IActionParamValidator
{
    public ValidatorType Type { get; set; }
    public Dictionary<string, string> ParamValuesByName { get; set; }

    public bool IsValid(string value, out string message);
}