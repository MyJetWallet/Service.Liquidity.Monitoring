using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.Grpc.Models.RuleSets
{
    [DataContract]
    public class GetMonitoringRuleSetResponse
    {
        [DataMember(Order = 1)] public MonitoringRuleSet Item { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public bool IsError { get; set; }
    }
}