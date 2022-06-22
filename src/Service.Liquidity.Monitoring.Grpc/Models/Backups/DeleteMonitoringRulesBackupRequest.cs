using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Grpc.Models.Backups
{
    [DataContract]
    public class DeleteMonitoringRulesBackupRequest
    {
        [DataMember(Order = 1)] public string Id { get; set; }
    }
}