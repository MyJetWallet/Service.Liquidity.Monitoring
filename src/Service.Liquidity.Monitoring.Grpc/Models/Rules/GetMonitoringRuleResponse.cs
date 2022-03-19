using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;

namespace Service.Liquidity.Monitoring.Grpc.Models.Rules
{
    [DataContract]
    public class GetMonitoringRuleResponse
    {
        [DataMember(Order = 1)] public MonitoringRule Item { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public bool IsError { get; set; }
    }
}