using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.Metrics;
using Service.Liquidity.Monitoring.Domain.Models.Metrics.Common;
using Service.Liquidity.Monitoring.Domain.Models.Operators;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets
{
    [DataContract]
    public class MonitoringRule
    {
        [DataMember(Order = 1)] public string Name { get; set; }
        [DataMember(Order = 2)] public IEnumerable<string> CheckIds { get; set; }
        [DataMember(Order = 3)] public LogicalOperatorType LogicalOperatorType { get; set; }
        [DataMember(Order = 4)] public string NotificationChannel { get; set; }
        [DataMember(Order = 5)] public MonitoringRuleAction MonitoringRuleAction { get; set; }
        [DataMember(Order = 6)] public MonitoringRuleState PrevState { get; set; }
        [DataMember(Order = 7)] public MonitoringRuleState CurrentState { get; set; }

        public bool Matches(Portfolio portfolio,
            IEnumerable<PortfolioCheck> checks,
            Dictionary<PortfolioMetricType, IPortfolioMetric> metrics)
        {
            var hashSet = CheckIds.ToHashSet();
            var ruleChecks = checks
                .Where(ch => hashSet.Contains(ch.Id))
                .ToList();

            if (ruleChecks.Count != hashSet.Count)
            {
                throw new Exception("Some of checks Not Found");
            }

            switch (LogicalOperatorType)
            {
                case LogicalOperatorType.All:
                {
                    var result = ruleChecks.All(ch => ch.Matches(portfolio, metrics));
                    PrevState = CurrentState;
                    CurrentState = new MonitoringRuleState(result);

                    return result;
                }
                case LogicalOperatorType.Any:
                {
                    var result = ruleChecks.Any(ch => ch.Matches(portfolio, metrics));
                    PrevState = CurrentState;
                    CurrentState = new MonitoringRuleState(result);

                    return result;
                }
                default: throw new NotSupportedException($"{nameof(LogicalOperatorType)}");
            }
        }
    }
}