using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Mapster;
using Service.Liquidity.Monitoring.Domain.Models.Actions;
using Service.Liquidity.Monitoring.Domain.Models.Checks;

namespace Service.Liquidity.Monitoring.Domain.Models.Rules
{
    [DataContract]
    public class MonitoringRule
    {
        [DataMember(Order = 8)] public string Id { get; set; }
        [DataMember(Order = 1)] public string Name { get; set; }
        [DataMember(Order = 3)] public RuleActivationType RuleActivationType { get; set; }
        [DataMember(Order = 6)] public MonitoringRuleState PrevState { get; set; }
        [DataMember(Order = 7)] public MonitoringRuleState CurrentState { get; set; }
        [DataMember(Order = 9)] public string Description { get; set; }
        [DataMember(Order = 11)] public IEnumerable<PortfolioCheck> Checks { get; set; }
        [DataMember(Order = 14)] public string MonitoringRuleSetId { get; set; }
        [DataMember(Order = 15)] public Dictionary<string, MonitoringAction> ActionsByTypeName { get; set; }

        public void RefreshState()
        {
            PrevState = CurrentState.Adapt<MonitoringRuleState>();
            var ruleChecks = Checks?.ToList() ?? new List<PortfolioCheck>();

            var isActive = ruleChecks.Any() && RuleActivationType switch
            {
                RuleActivationType.WhenAllChecksActive => ruleChecks.All(ch => ch.CurrentState.IsActive),
                RuleActivationType.WhenAnyCheckActive => ruleChecks.Any(ch => ch.CurrentState.IsActive),
                _ => throw new NotSupportedException($"{nameof(RuleActivationType)}")
            };
            var activeCheckIds = ruleChecks
                .Where(ch => ch.CurrentState.IsActive)
                .Select(ch => ch.Id);

            if (CurrentState == null)
            {
                CurrentState = MonitoringRuleState.Create(isActive, activeCheckIds);
            }
            else
            {
                CurrentState.Refresh(isActive, activeCheckIds);
            }
        }
    }
}