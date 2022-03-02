using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;

namespace Service.Liquidity.Monitoring.Grpc.Models.RuleSets
{
    [DataContract]
    public class GetMonitoringRuleSetListResponse
    {
        [DataMember(Order = 1)] public IEnumerable<MonitoringRuleSet> Items { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public bool IsError { get; set; }
    }
}