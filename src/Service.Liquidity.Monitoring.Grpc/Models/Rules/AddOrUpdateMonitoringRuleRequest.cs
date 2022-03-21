using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.Grpc.Models.Rules
{
    [DataContract]
    public class AddOrUpdateMonitoringRuleRequest
    {
        [DataMember(Order = 1)] public MonitoringRule Item { get; set; }
    }
}