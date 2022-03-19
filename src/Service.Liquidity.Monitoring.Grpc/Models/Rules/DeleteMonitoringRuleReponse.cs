using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Grpc.Models.Rules
{
    [DataContract]
    public class DeleteMonitoringRuleResponse
    {
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 1)] public bool IsError { get; set; }
    }
}