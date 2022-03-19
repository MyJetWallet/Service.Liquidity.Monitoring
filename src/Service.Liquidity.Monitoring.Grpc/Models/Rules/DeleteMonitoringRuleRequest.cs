using System.Runtime.Serialization;
using Autofac.Core;

namespace Service.Liquidity.Monitoring.Grpc.Models.Rules
{
    [DataContract]
    public class DeleteMonitoringRuleRequest
    {
        [DataMember(Order = 1)] public string MonitoringRuleId { get; set; }
    }
}