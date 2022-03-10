using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Interfaces;

public interface IHedgeService
{
    public Task HedgeAsync(Portfolio portfolio, IEnumerable<MonitoringRuleSet> ruleSets);
}