using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Grpc.Models.Backups
{
    [DataContract]
    public class DeleteMonitoringRulesBackupResponse
    {
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 1)] public bool IsError { get; set; }
    }
}