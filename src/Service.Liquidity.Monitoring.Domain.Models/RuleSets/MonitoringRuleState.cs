using System;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets
{
    [DataContract]
    public class MonitoringRuleState
    {
        [DataMember(Order = 1)] public DateTime Date { get; set; }
        [DataMember(Order = 2)] public bool IsActive { get; set; }

        public MonitoringRuleState(bool isActive)
        {
            Date = DateTime.UtcNow;
            IsActive = isActive;
        }
    }
}