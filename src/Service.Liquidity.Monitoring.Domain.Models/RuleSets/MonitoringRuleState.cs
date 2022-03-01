using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets
{
    [DataContract]
    public class MonitoringRuleState
    {
        [DataMember(Order = 1)] public DateTime Date { get; set; }
        [DataMember(Order = 2)] public bool IsActive { get; set; }
        [DataMember(Order = 3)] public IEnumerable<string> ActiveCheckIds { get; set; }
        [DataMember(Order = 4)] public DateTime? IsActiveChangedDate { get; set; }
        [DataMember(Order = 5)] public DateTime? NotificationSendDate { get; set; }

        public MonitoringRuleState()
        {
        }

        public MonitoringRuleState(bool isActive, DateTime? isActiveChangedDate, IEnumerable<string> activeCheckIds = null)
        {
            Date = DateTime.UtcNow;
            IsActive = isActive;
            ActiveCheckIds = activeCheckIds ?? ArraySegment<string>.Empty;
            IsActiveChangedDate = isActiveChangedDate;
        }
    }
}