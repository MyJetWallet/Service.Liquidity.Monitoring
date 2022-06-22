using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.Grpc.Models.Backups
{
    [DataContract]
    public class AddOrUpdateMonitoringRulesBackupRequest
    {
        [DataMember(Order = 1)] public MonitoringRulesBackup Item { get; set; }
    }
}