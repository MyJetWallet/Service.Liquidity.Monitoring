using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.Rules;

[DataContract]
public class MonitoringRulesBackupInfo
{
    [DataMember(Order = 1)] public string BackupId { get; set; }
    [DataMember(Order = 2)] public string BackupName { get; set; }
    [DataMember(Order = 3)] public DateTime BackupCreatedDate { get; set; }
    [DataMember(Order = 5)] public string BackupCreatedBy { get; set; }
}