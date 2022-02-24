using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets
{
    [DataContract]
    public class MonitoringRule
    {
        [DataMember(Order = 1)] public string Name { get; set; }
        [DataMember(Order = 2)] public IEnumerable<string> CheckIds { get; set; }
        [DataMember(Order = 3)] public ConcatenationType ConcatenationType { get; set; }
        [DataMember(Order = 4)] public string NotificationChannel { get; set; }
        [DataMember(Order = 5)] public MonitoringRuleAction MonitoringRuleAction { get; set; }
        [DataMember(Order = 6)] public MonitoringRuleState PrevState { get; set; }
        [DataMember(Order = 7)] public MonitoringRuleState CurrentState { get; set; }

        public bool Matches(Portfolio portfolio, IEnumerable<PortfolioCheck> checks)
        {
            var checkIds = CheckIds.ToHashSet();
            var ruleChecks = checks
                .Where(ch => checkIds.Contains(ch.Id))
                .ToList();

            if (ruleChecks.Count != checkIds.Count)
            {
                throw new Exception("Some of checks Not Found");
            }

            switch (ConcatenationType)
            {
                case ConcatenationType.All:
                {
                    var result = ruleChecks.All(ch => ch.Matches(portfolio));
                    PrevState = CurrentState;
                    CurrentState = new MonitoringRuleState(result);

                    return result;
                }
                case ConcatenationType.Any:
                {
                    var result = ruleChecks.Any(ch => ch.Matches(portfolio));
                    PrevState = CurrentState;
                    CurrentState = new MonitoringRuleState(result);

                    return result;
                }
                default: throw new NotSupportedException($"{nameof(ConcatenationType)}");
            }
        }
    }
}