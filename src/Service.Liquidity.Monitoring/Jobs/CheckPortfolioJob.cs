using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;
using Service.Liquidity.TradingPortfolio.Grpc;

namespace Service.Liquidity.Monitoring.Jobs
{
    public class CheckPortfolioJob : IStartable
    {
        private readonly ILogger<CheckPortfolioJob> _logger;
        private readonly IPortfolioChecksExecutor _portfolioChecksExecutor;
        private readonly IMonitoringRuleSetsExecutor _monitoringRuleSetsExecutor;
        private readonly IServiceBusPublisher<PortfolioMonitoringMessage> _publisher;
        private readonly IManualInputService _portfolioService;
        private readonly IMonitoringRulesStorage _monitoringRulesStorage;
        private readonly IPortfolioMetricsFactory _portfolioMetricsFactory;
        private readonly MyTaskTimer _timer;

        public CheckPortfolioJob(
            ILogger<CheckPortfolioJob> logger,
            IPortfolioChecksExecutor portfolioChecksExecutor,
            IMonitoringRuleSetsExecutor monitoringRuleSetsExecutor,
            IServiceBusPublisher<PortfolioMonitoringMessage> publisher,
            IManualInputService portfolioService,
            IMonitoringRulesStorage monitoringRulesStorage,
            IPortfolioMetricsFactory portfolioMetricsFactory
        )
        {
            _logger = logger;
            _portfolioChecksExecutor = portfolioChecksExecutor;
            _monitoringRuleSetsExecutor = monitoringRuleSetsExecutor;
            _publisher = publisher;
            _portfolioService = portfolioService;
            _monitoringRulesStorage = monitoringRulesStorage;
            _portfolioMetricsFactory = portfolioMetricsFactory;
            _timer = new MyTaskTimer(nameof(CheckPortfolioJob),
                    TimeSpan.FromMilliseconds(5000),
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
                var portfolio = (await _portfolioService.GetPortfolioAsync()).Portfolio;

                if (portfolio == null)
                {
                    _logger.LogWarning($"Can't do {nameof(CheckPortfolioJob)}. Portfolio Not found");
                    return;
                }

                var checks = await _portfolioChecksExecutor.ExecuteAsync(portfolio);

                if (checks == null || !checks.Any())
                {
                    return;
                }

                var ruleSets = await _monitoringRuleSetsExecutor.ExecuteAsync(portfolio, checks);

                if (ruleSets == null || !ruleSets.Any())
                {
                    return;
                }

                var rules = (await _monitoringRulesStorage.GetAsync())?.ToList();

                foreach (var rule in rules ?? new List<MonitoringRule>())
                {
                    foreach (var check in rule.Checks ?? new List<PortfolioCheck>())
                    {
                        check.RefreshState(portfolio, _portfolioMetricsFactory.Get(check.MetricType));
                    }
                    
                    rule.RefreshState();
                }

                await _monitoringRulesStorage.UpdateStatesAsync(rules);

                await _publisher.PublishAsync(new PortfolioMonitoringMessage
                {
                    Portfolio = portfolio,
                    Checks = checks,
                    RuleSets = ruleSets,
                    Rules = rules
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{job} failed", nameof(CheckPortfolioJob));
            }
        }
    }
}