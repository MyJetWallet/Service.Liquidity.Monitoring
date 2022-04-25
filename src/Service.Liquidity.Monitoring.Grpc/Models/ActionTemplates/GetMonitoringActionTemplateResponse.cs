using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Actions;
using Service.Liquidity.Monitoring.Domain.Models.Actions.Templates;

namespace Service.Liquidity.Monitoring.Grpc.Models.ActionTemplates;

[DataContract]
public class GetMonitoringActionTemplateResponse
{
    [DataMember(Order = 1)] public bool IsError { get; set; }
    [DataMember(Order = 2)] public string ErrorMessage { get; set; }
    [DataMember(Order = 3)] public MonitoringActionTemplate Template { get; set; }
}