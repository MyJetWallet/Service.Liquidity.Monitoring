using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.Grpc.Models.RuleSets
{
    [DataContract]
    public class AddOrUpdateMonitoringRuleSetRequest
    {
        [DataMember(Order = 1)] public MonitoringRuleSet Item { get; set; }
    }
}