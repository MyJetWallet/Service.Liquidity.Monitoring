using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.Metrics.Common;
using Service.Liquidity.Monitoring.Domain.Services;
using Service.Liquidity.TradingPortfolio.Domain.Models.NoSql;

namespace Service.Liquidity.Monitoring.Jobs
{
    public class ExecutePortfolioChecksJob : IStartable
    {
        private readonly ILogger<ExecutePortfolioChecksJob> _logger;
        private readonly IMyNoSqlServerDataReader<PortfolioNoSql> _portfolioReader;
        private readonly IPortfolioChecksStorage _portfolioChecksStorage;
        private readonly IPortfolioMetricsFactory _portfolioMetricsFactory;
        private readonly MyTaskTimer _timer;

        public ExecutePortfolioChecksJob(
            ILogger<ExecutePortfolioChecksJob> logger,
            IMyNoSqlServerDataReader<PortfolioNoSql> portfolioReader,
            IPortfolioChecksStorage portfolioChecksStorage,
            IPortfolioMetricsFactory portfolioMetricsFactory
        )
        {
            _logger = logger;
            _portfolioReader = portfolioReader;
            _portfolioChecksStorage = portfolioChecksStorage;
            _portfolioMetricsFactory = portfolioMetricsFactory;
            _timer = new MyTaskTimer(nameof(ExecutePortfolioChecksJob),
                    TimeSpan.FromMilliseconds(1000),
                    logger,
                    DoTimeAsync)
                .DisableTelemetry();
        }

        public void Start()
        {
            _timer.Start();
        }

        private async Task DoTimeAsync()
        {
            try
            {
                var portfolio = _portfolioReader.Get().FirstOrDefault()?.Portfolio;

                if (portfolio == null)
                {
                    _logger.LogWarning("Can't {job}. Portfolio Not found", nameof(ExecutePortfolioChecksJob));
                    return;
                }

                var checks = (await _portfolioChecksStorage.GetAsync())?.ToList() ?? new List<PortfolioCheck>();

                if (!checks.Any())
                {
                    _logger.LogWarning("Can't {job}. Checks Not Found", nameof(ExecutePortfolioChecksJob));
                    return;
                }

                var metrics = _portfolioMetricsFactory.Get()?.ToList() ?? new List<IPortfolioMetric>();

                if (!metrics.Any())
                {
                    _logger.LogWarning("Can't {job}. Metrics Not Found", nameof(ExecutePortfolioChecksJob));
                    return;
                }

                foreach (var check in checks)
                {
                    check.Execute(portfolio, metrics.FirstOrDefault(m => m.Type == check.MetricType));
                }

                await _portfolioChecksStorage.BulkInsetOrUpdateAsync(checks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed {job}", nameof(ExecutePortfolioChecksJob));
            }
        }
    }
}