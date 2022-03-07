using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.Metrics.Common;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Services
{
    public class PortfolioChecksExecutor : IPortfolioChecksExecutor
    {
        private readonly ILogger<PortfolioChecksExecutor> _logger;
        private readonly IPortfolioChecksStorage _portfolioChecksStorage;
        private readonly IPortfolioMetricsFactory _portfolioMetricsFactory;

        public PortfolioChecksExecutor(
            ILogger<PortfolioChecksExecutor> logger,
            IPortfolioChecksStorage portfolioChecksStorage,
            IPortfolioMetricsFactory portfolioMetricsFactory
        )
        {
            _logger = logger;
            _portfolioChecksStorage = portfolioChecksStorage;
            _portfolioMetricsFactory = portfolioMetricsFactory;
        }

        public async Task<IEnumerable<PortfolioCheck>> ExecuteAsync(Portfolio portfolio)
        {
            var checks = (await _portfolioChecksStorage.GetAsync())?.ToList() ?? new List<PortfolioCheck>();

            if (!checks.Any())
            {
                _logger.LogWarning("Can't ExecutePortfolioChecks. Checks Not Found");

                return ArraySegment<PortfolioCheck>.Empty;
            }

            var metrics = _portfolioMetricsFactory.Get()?.ToList() ?? new List<IPortfolioMetric>();

            if (!metrics.Any())
            {
                _logger.LogWarning("Can't ExecutePortfolioChecks. Metrics Not Found");

                return ArraySegment<PortfolioCheck>.Empty;
            }

            foreach (var check in checks)
            {
                check.Execute(portfolio, metrics.FirstOrDefault(m => m.Type == check.MetricType));
            }

            await _portfolioChecksStorage.BulkInsetOrUpdateAsync(checks);

            return checks;
        }
    }
}