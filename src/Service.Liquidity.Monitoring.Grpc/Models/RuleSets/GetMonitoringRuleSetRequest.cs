using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Grpc.Models.RuleSets
{
    [DataContract]
    public class GetMonitoringRuleSetRequest
    {
        [DataMember(Order = 1)] public string Id { get; set; }
    }
}