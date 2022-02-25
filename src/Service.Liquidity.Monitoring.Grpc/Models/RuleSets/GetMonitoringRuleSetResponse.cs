using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;

namespace Service.Liquidity.Monitoring.Grpc.Models.RuleSets
{
    [DataContract]
    public class GetMonitoringRuleSetResponse
    {
        [DataMember(Order = 1)] public MonitoringRuleSet Item { get; set; }
    }
}