using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using MyJetWallet.Sdk.ServiceBus;
using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models.NoSql;

namespace Service.Liquidity.Monitoring.Jobs
{
    public class ExecuteRuleSetsJob : IStartable
    {
        private readonly ILogger<ExecuteRuleSetsJob> _logger;
        private readonly IPortfolioChecksExecutor _portfolioChecksExecutor;
        private readonly IMonitoringRuleSetsExecutor _monitoringRuleSetsExecutor;
        private readonly IMyNoSqlServerDataReader<PortfolioNoSql> _portfolioReader;
        private readonly IServiceBusPublisher<PortfolioMonitoringMessage> _publisher;
        private readonly MyTaskTimer _timer;

        public ExecuteRuleSetsJob(
            ILogger<ExecuteRuleSetsJob> logger,
            IPortfolioChecksExecutor portfolioChecksExecutor,
            IMonitoringRuleSetsExecutor monitoringRuleSetsExecutor,
            IMyNoSqlServerDataReader<PortfolioNoSql> portfolioReader,
            IServiceBusPublisher<PortfolioMonitoringMessage> publisher
        )
        {
            _logger = logger;
            _portfolioChecksExecutor = portfolioChecksExecutor;
            _monitoringRuleSetsExecutor = monitoringRuleSetsExecutor;
            _portfolioReader = portfolioReader;
            _publisher = publisher;
            _timer = new MyTaskTimer(nameof(ExecuteRuleSetsJob),
                    TimeSpan.FromMilliseconds(3000),
                    logger,
                    DoAsync)
                .DisableTelemetry();
        }

        public void Start()
        {
            _timer.Start();
        }

        private async Task DoAsync()
        {
            try
            {
                var portfolio = _portfolioReader.Get().FirstOrDefault()?.Portfolio;

                if (portfolio == null)
                {
                    _logger.LogWarning("Can't ExecuteRuleSetsJob. Portfolio Not found");
                    return;
                }

                var checks = await _portfolioChecksExecutor.ExecuteAsync(portfolio);

                if (checks == null || !checks.Any())
                {
                    _logger.LogWarning("Can't ExecuteRuleSetsJob. Checks Not found");
                    return;
                }

                var ruleSets = await _monitoringRuleSetsExecutor.ExecuteAsync(portfolio, checks);

                if (ruleSets == null || !ruleSets.Any())
                {
                    _logger.LogWarning("Can't ExecuteRuleSetsJob. RuleSets Not found");
                    return;
                }

                await _publisher.PublishAsync(new PortfolioMonitoringMessage
                {
                    Portfolio = portfolio,
                    Checks = checks,
                    RuleSets = ruleSets
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{job} failed", nameof(ExecuteRuleSetsJob));
            }
        }
    }
}