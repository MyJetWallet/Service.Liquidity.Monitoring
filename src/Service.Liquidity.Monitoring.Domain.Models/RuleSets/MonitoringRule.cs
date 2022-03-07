using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.Hedging.Common;
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

        public void SetNotificationSendDate(DateTime date)
        {
            CurrentState.NotificationSendDate = date;
        }

        public bool IsNeedNotification(bool isActiveChanged)
        {
            if (!IsNotificationEnabled())
            {
                return false;
            }

            if (CurrentState.NotificationSendDate == null)
            {
                return true;
            }

            if (isActiveChanged)
            {
                return true;
            }

            var timeToRemind = DateTime.UtcNow - CurrentState.NotificationSendDate > TimeSpan.FromMinutes(60);

            return CurrentState.IsActive && timeToRemind;
        }

        public string GetNotificationText(IEnumerable<PortfolioCheck> checks)
        {
            var ruleChecks = Filter(checks);
            var title =
                $"Rule <b>{Name}</b> is {(CurrentState.IsActive ? "active" : "inactive")}:{Environment.NewLine}{Description}";
            var checkDescriptions = ruleChecks.Select(ch => ch.GenerateDescription());
            var body = string.Join($"{Environment.NewLine}", checkDescriptions);

            return $"{title}{Environment.NewLine}" +
                   $"{body}{Environment.NewLine}{Environment.NewLine}" +
                   $"Date: {CurrentState.Date:yyyy-MM-dd hh:mm:ss}";
        }

        public bool Execute(IEnumerable<PortfolioCheck> checks)
        {
            var ruleChecks = Filter(checks).ToArray();

            var isActive = LogicalOperatorType switch
            {
                LogicalOperatorType.All => ruleChecks.All(ch => ch.CurrentState.IsActive),
                LogicalOperatorType.Any => ruleChecks.Any(ch => ch.CurrentState.IsActive),
                _ => throw new NotSupportedException($"{nameof(LogicalOperatorType)}")
            };

            var isChanged = false;
            var isActiveChangedDate = CurrentState?.IsActiveChangedDate;

            if (CurrentState?.IsActive != isActive)
            {
                isChanged = true;
                isActiveChangedDate = DateTime.UtcNow;
                PrevState = CurrentState;
            }

            var activeCheckIds = ruleChecks
                .Where(ch => ch.CurrentState.IsActive)
                .Select(ch => ch.Id);
            CurrentState = new MonitoringRuleState(isActive,
                isActiveChangedDate,
                CurrentState?.NotificationSendDate,
                activeCheckIds);

            return isChanged;
        }

        private bool IsNotificationEnabled()
        {
            return !string.IsNullOrWhiteSpace(NotificationChannelId);
        }

        private IEnumerable<PortfolioCheck> Filter(IEnumerable<PortfolioCheck> checks)
        {
            var hashSet = CheckIds.ToHashSet();
            var ruleChecks = checks
                .Where(ch => hashSet.Contains(ch.Id))
                .ToList();

            if (ruleChecks.Count != hashSet.Count)
            {
                throw new Exception($"Not all checks found for Rule {Name}");
            }

            return ruleChecks;
        }
    }
}