using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.Rules;

[DataContract]
public class MonitoringRulesBackup
{
    [DataMember(Order = 1)] public string Id { get; set; }
    [DataMember(Order = 2)] public string Name { get; set; }
    [DataMember(Order = 3)] public DateTime CreatedDate { get; set; }
    [DataMember(Order = 4)] public List<MonitoringRule> MonitoringRules { get; set; }
    [DataMember(Order = 5)] public string CreatedBy { get; set; }
}