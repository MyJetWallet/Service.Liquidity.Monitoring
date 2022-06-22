using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.Grpc.Models.Backups
{
    [DataContract]
    public class GetMonitoringRulesBackupResponse
    {
        [DataMember(Order = 1)] public MonitoringRulesBackup Item { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public bool IsError { get; set; }
    }
}