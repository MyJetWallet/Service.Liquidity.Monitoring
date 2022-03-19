using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Grpc.Models.Rules
{
    [DataContract]
    public class GetMonitoringRuleRequest
    {
        [DataMember(Order = 1)] public string MonitoringRuleId { get; set; }
    }
}