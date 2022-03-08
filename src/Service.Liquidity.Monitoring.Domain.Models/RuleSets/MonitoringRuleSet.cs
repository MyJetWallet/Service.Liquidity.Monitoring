using System.Collections.Generic;
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

        public void OrderRules()
        {
            var orderedRules = Rules
                .OrderByDescending(r => r.CurrentState.HedgeParams.SellAssets.FirstOrDefault()?.SellAmountInUsd)
                .ToList();

            foreach (var rule in Rules)
            {
                if (rule.HedgeStrategyType == HedgeStrategyType.Return)
                {
                    orderedRules.Insert(0, rule); // return rules is the most prioritized
                }
            }

            Rules = orderedRules;
        }
    }
}