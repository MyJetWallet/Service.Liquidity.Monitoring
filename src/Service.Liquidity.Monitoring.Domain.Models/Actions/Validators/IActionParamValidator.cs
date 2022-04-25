namespace Service.Liquidity.Monitoring.Domain.Models.Actions.Validators;

public interface IActionParamValidator
{
    bool IsValid(string value, out string message);
}