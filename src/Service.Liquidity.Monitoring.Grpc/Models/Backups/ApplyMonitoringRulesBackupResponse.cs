using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.Grpc.Models.Backups
{
    [DataContract]
    public class ApplyMonitoringRulesBackupResponse
    {
        [DataMember(Order = 1)] public string ErrorMessage { get; set; }
        [DataMember(Order = 2)] public bool IsError { get; set; }
    }
}