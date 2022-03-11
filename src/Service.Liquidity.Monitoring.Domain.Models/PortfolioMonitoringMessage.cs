using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Models
{
    [DataContract]
    public class PortfolioMonitoringMessage
    {
        public const string TopicName = "jetwallet-liquidity-portfolio-monitoring-message";

        [DataMember(Order = 1)] public Portfolio Portfolio { get; set; }
        [DataMember(Order = 2)] public ICollection<PortfolioCheck> Checks { get; set; }
        [DataMember(Order = 3)] public ICollection<MonitoringRuleSet> RuleSets { get; set; }
    }
}