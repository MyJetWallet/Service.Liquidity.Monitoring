using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.Grpc.Models.Backups
{
    [DataContract]
    public class GetMonitoringRulesBackupInfosResponse
    {
        [DataMember(Order = 1)] public IEnumerable<MonitoringRulesBackupInfo> Items { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public bool IsError { get; set; }
    }
}