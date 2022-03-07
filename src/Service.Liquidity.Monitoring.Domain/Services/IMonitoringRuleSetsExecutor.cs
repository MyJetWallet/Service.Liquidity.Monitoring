﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Services;

public interface IMonitoringRuleSetsExecutor
{
    public Task ExecuteAsync(Portfolio portfolio, IEnumerable<PortfolioCheck> portfolioChecks);
}