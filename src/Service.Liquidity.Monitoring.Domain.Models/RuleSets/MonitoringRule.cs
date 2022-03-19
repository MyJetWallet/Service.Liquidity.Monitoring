using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Mapster;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.Operators;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets
{
    [DataContract]
    public class MonitoringRule
    {
        [DataMember(Order = 8)] public string Id { get; set; }
        [DataMember(Order = 1)] public string Name { get; set; }
        [DataMember(Order = 2)] public IEnumerable<string> CheckIds { get; set; }
        [DataMember(Order = 3)] public LogicalOperatorType LogicalOperatorType { get; set; }
        [DataMember(Order = 4)] public string NotificationChannelId { get; set; }
        [DataMember(Order = 5)] public HedgeStrategyType HedgeStrategyType { get; set; }
        [DataMember(Order = 6)] public MonitoringRuleState PrevState { get; set; }
        [DataMember(Order = 7)] public MonitoringRuleState CurrentState { get; set; }
        [DataMember(Order = 9)] public string Description { get; set; }
        [DataMember(Order = 10)] public HedgeStrategyParams HedgeStrategyParams { get; set; }
        [DataMember(Order = 11)] public IEnumerable<PortfolioCheck> Checks { get; set; }
        [DataMember(Order = 12)] public string Category { get; set; }
        [DataMember(Order = 13)] public Dictionary<string, CustomParam> ParamsByName { get; set; }
        [DataMember(Order = 14)] public string MonitoringRuleSetId { get; set; }

        public void RefreshState()
        {
            RefreshState(Checks);
        }

        public void RefreshState(IEnumerable<PortfolioCheck> checks)
        {
            PrevState = CurrentState.Adapt<MonitoringRuleState>();
            var ruleChecks = Filter(checks ?? new List<PortfolioCheck>());

            var isActive = LogicalOperatorType switch
            {
                LogicalOperatorType.All => ruleChecks.All(ch => ch.CurrentState.IsActive),
                LogicalOperatorType.Any => ruleChecks.Any(ch => ch.CurrentState.IsActive),
                _ => throw new NotSupportedException($"{nameof(LogicalOperatorType)}")
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

        private List<PortfolioCheck> Filter(IEnumerable<PortfolioCheck> checks)
        {
            var hashSet = CheckIds?.ToHashSet() ?? new HashSet<string>();
            var ruleChecks = checks?
                .Where(ch => hashSet.Contains(ch.Id))
                .ToList() ?? new List<PortfolioCheck>();

            return ruleChecks;
        }
    }
}