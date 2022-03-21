using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.Grpc.Models.Rules
{
    [DataContract]
    public class GetMonitoringRuleListResponse
    {
        [DataMember(Order = 1)] public IEnumerable<MonitoringRule> Items { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public bool IsError { get; set; }
    }
}