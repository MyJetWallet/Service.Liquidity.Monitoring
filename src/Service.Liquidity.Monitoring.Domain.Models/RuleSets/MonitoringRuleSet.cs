using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.Hedging.Common;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets
{
    [DataContract]
    public class MonitoringRuleSet
    {
        [DataMember(Order = 1)] public string Id { get; set; }
        [DataMember(Order = 2)] public string Name { get; set; }
        [DataMember(Order = 3)] public IEnumerable<MonitoringRule> Rules { get; set; }

        public bool NeedsHedging()
        {
            if (Rules == null || !Rules.Any())
            {
                return false;
            }

            var activeRules = Rules.Where(rule => rule.CurrentState.IsActive && rule.NeedsHedging()).ToList();

            if (!activeRules.Any())
            {
                return false;
            }

            if (activeRules.Any(rule => rule.HedgeStrategyType == HedgeStrategyType.Return))
            {
                return false;
            }

            return true;
        }

        public void OrderRules()
        {
            var orderedRules = Rules
                .OrderByDescending(r => r.CurrentState.HedgeParams.BuyAmount)
                .ToList();

            foreach (var rule in Rules)
            {
                if (rule.HedgeStrategyType == HedgeStrategyType.Return)
                {
                    orderedRules.Remove(rule);
                    orderedRules.Insert(0, rule); // return rules is the most prioritized
                }
            }

            Rules = orderedRules;
        }
    }
}