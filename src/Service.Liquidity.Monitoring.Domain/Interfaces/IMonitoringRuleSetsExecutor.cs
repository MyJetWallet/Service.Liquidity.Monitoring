using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Interfaces;

public interface IMonitoringRuleSetsExecutor
{
    public Task<ICollection<MonitoringRuleSet>> ExecuteAsync(Portfolio portfolio,
        IEnumerable<PortfolioCheck> portfolioChecks);
}