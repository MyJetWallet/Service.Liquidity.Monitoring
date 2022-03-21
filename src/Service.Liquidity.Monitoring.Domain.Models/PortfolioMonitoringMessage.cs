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
        [DataMember(Order = 4)] public ICollection<MonitoringRule> Rules { get; set; }

    }
}