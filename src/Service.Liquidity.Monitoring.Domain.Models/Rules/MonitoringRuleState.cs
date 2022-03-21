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
        [DataMember(Order = 4)] public DateTime IsActiveChangedDate { get; set; }

        public static MonitoringRuleState Create(bool isActive,
            IEnumerable<string> activeCheckIds = null)
        {
            return new MonitoringRuleState
            {
                Date = DateTime.UtcNow,
                IsActive = isActive,
                ActiveCheckIds = activeCheckIds ?? Array.Empty<string>(),
                IsActiveChangedDate = DateTime.UtcNow,
            };
        }

        public void Refresh(bool isActive,
            IEnumerable<string> activeCheckIds = null
        )
        {
            Date = DateTime.UtcNow;

            if (IsActive != isActive)
            {
                IsActiveChangedDate = Date;
            }

            IsActive = isActive;
            ActiveCheckIds = activeCheckIds ?? Array.Empty<string>();
        }
    }
}