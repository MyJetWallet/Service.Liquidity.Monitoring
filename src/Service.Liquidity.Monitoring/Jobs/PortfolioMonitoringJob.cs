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
using Service.Liquidity.Monitoring.Domain.Models.Rules;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using Service.Liquidity.TradingPortfolio.Grpc;

namespace Service.Liquidity.Monitoring.Jobs
{
    public class PortfolioMonitoringJob : IStartable
    {
        private readonly ILogger<PortfolioMonitoringJob> _logger;
        private readonly IServiceBusPublisher<PortfolioMonitoringMessage> _publisher;
        private readonly IManualInputService _portfolioService;
        private readonly IMonitoringRulesStorage _monitoringRulesStorage;
        private readonly IPortfolioMetricsFactory _portfolioMetricsFactory;
        private readonly MyTaskTimer _timer;

        public PortfolioMonitoringJob(
            ILogger<PortfolioMonitoringJob> logger,
            IServiceBusPublisher<PortfolioMonitoringMessage> publisher,
            IManualInputService portfolioService,
            IMonitoringRulesStorage monitoringRulesStorage,
            IPortfolioMetricsFactory portfolioMetricsFactory
        )
        {
            _logger = logger;
            _publisher = publisher;
            _portfolioService = portfolioService;
            _monitoringRulesStorage = monitoringRulesStorage;
            _portfolioMetricsFactory = portfolioMetricsFactory;
            _timer = new MyTaskTimer(nameof(PortfolioMonitoringJob),
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
                var portfolio = (await _portfolioService.GetPortfolioAsync())?.Portfolio;

                if (portfolio == null)
                {
                    _logger.LogWarning($"Can't do {nameof(PortfolioMonitoringJob)}. Portfolio Not found");
                    return;
                }

                var rules = (await _monitoringRulesStorage.GetAsync())?.ToList();
                
                await RefreshAsync(portfolio, rules);
                await PublishAsync(portfolio, rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{job} failed", nameof(PortfolioMonitoringJob));
            }
        }

        private async Task RefreshAsync(Portfolio portfolio, ICollection<MonitoringRule> rules)
        {
            try
            {
                foreach (var rule in rules ?? new List<MonitoringRule>())
                {
                    foreach (var check in rule.Checks ?? new List<PortfolioCheck>())
                    {
                        check.RefreshState(portfolio, _portfolioMetricsFactory.Get(check.MetricType));
                    }
                    
                    rule.RefreshState();
                }

                await _monitoringRulesStorage.UpdateStatesAsync(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to RefreshRules");
            }
        }

        private async Task PublishAsync(Portfolio portfolio, ICollection<MonitoringRule> rules)
        {
            try
            {
                if (rules?.Any() ?? false)
                {
                    var message = new PortfolioMonitoringMessage
                    {
                        Portfolio = portfolio,
                        Rules = rules
                    };
                    
                    await _publisher.PublishAsync(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to Publish {nameof(PortfolioMonitoringMessage)}");
            }
        }
    }
}