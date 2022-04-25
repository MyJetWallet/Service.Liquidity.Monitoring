using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Actions;

namespace Service.Liquidity.Monitoring.Grpc.Models.ActionTemplates;

[DataContract]
public class GetMonitoringActionTemplateListRequest
{
    [DataMember(Order = 1)] public MonitoringAction Action { get; set; }
}