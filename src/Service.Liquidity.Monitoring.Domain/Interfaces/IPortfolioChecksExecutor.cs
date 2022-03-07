using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Interfaces;

public interface IPortfolioChecksExecutor
{
    public Task<IEnumerable<PortfolioCheck>> ExecuteAsync(Portfolio portfolio);

}