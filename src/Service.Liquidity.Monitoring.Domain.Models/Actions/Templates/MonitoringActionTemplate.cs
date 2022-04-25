using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.Actions.Templates;

[DataContract]
public class MonitoringActionTemplate
{
    [DataMember(Order = 1)] public IMonitoringAction Action { get; set; } = new MonitoringAction();

    [DataMember(Order = 2)] public ICollection<MonitoringActionParamTemplate> ParamTemplates { get; set; } =
        new List<MonitoringActionParamTemplate>();

    public MonitoringAction ToAction()
    {
        return new MonitoringAction
        {
            TypeName = Action.TypeName,
            ParamInfos = Action.ParamInfos,
            ParamValuesByName = ParamTemplates
                .ToDictionary(paramTemplate => paramTemplate.Name, paramTemplate => paramTemplate.Value)
        };
    }

    public bool Validate(out string message)
    {
        message = "";

        foreach (var paramTemplate in ParamTemplates)
        {
            if (!paramTemplate.Validate(out var paramMessage))
            {
                message += $"{paramMessage}; ";
            }
        }

        return string.IsNullOrEmpty(message);
    }
}