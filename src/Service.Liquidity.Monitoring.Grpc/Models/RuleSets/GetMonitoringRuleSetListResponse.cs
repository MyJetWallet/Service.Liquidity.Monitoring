using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;

namespace Service.Liquidity.Monitoring.Grpc.Models.RuleSets
{
    [DataContract]
    public class GetMonitoringRuleSetListResponse
    {
        [DataMember(Order = 1)] public IEnumerable<MonitoringRuleSet> Items { get; set; }
    }
}