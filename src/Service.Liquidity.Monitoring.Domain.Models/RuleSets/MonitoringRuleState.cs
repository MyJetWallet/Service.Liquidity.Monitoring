using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Hedging.Common;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets
{
    [DataContract]
    public class MonitoringRuleState
    {
        [DataMember(Order = 1)] public DateTime Date { get; set; }
        [DataMember(Order = 2)] public bool IsActive { get; set; }
        [DataMember(Order = 3)] public IEnumerable<string> ActiveCheckIds { get; set; }
        [DataMember(Order = 4)] public DateTime IsActiveChangedDate { get; set; }
        [DataMember(Order = 6)] public HedgeParams HedgeParams { get; set; }

        public static MonitoringRuleState Create(bool isActive,
            HedgeParams hedgeParams,
            IEnumerable<string> activeCheckIds = null)
        {
            return new MonitoringRuleState
            {
                Date = DateTime.UtcNow,
                HedgeParams = hedgeParams,
                IsActive = isActive,
                ActiveCheckIds = activeCheckIds ?? Array.Empty<string>(),
                IsActiveChangedDate = DateTime.UtcNow,
            };
        }

        public void Refresh(bool isActive,
            HedgeParams hedgeParams,
            IEnumerable<string> activeCheckIds = null
        )
        {
            Date = DateTime.UtcNow;

            if (IsActive != isActive)
            {
                IsActiveChangedDate = Date;
            }

            IsActive = isActive;
            HedgeParams = hedgeParams;
            ActiveCheckIds = activeCheckIds ?? Array.Empty<string>();
        }
    }
}