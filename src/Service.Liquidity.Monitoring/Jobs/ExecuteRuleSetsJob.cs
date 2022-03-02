using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;
using Service.Liquidity.Monitoring.Domain.Services;

namespace Service.Liquidity.Monitoring.Jobs
{
    public class ExecuteRuleSetsJob : IStartable
    {
        private readonly ILogger<ExecutePortfolioChecksJob> _logger;
        private readonly IMonitoringRuleSetsStorage _ruleSetsStorage;
        private readonly IPortfolioChecksStorage _portfolioChecksStorage;
        private readonly IServiceBusPublisher<MonitoringNotificationMessage> _notificationPublisher;
        private readonly MyTaskTimer _timer;

        public ExecuteRuleSetsJob(
            ILogger<ExecutePortfolioChecksJob> logger,
            IMonitoringRuleSetsStorage ruleSetsStorage,
            IPortfolioChecksStorage portfolioChecksStorage,
            IServiceBusPublisher<MonitoringNotificationMessage> notificationPublisher
        )
        {
            _logger = logger;
            _ruleSetsStorage = ruleSetsStorage;
            _portfolioChecksStorage = portfolioChecksStorage;
            _notificationPublisher = notificationPublisher;
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
                var ruleSets = (await _ruleSetsStorage.GetAsync())?.ToList() ?? new List<MonitoringRuleSet>();

                if (!ruleSets.Any())
                {
                    _logger.LogWarning("Can't do {job}. RuleSets not found", nameof(ExecuteRuleSetsJob));
                }

                var checks = (await _portfolioChecksStorage.GetAsync())?.ToList() ?? new List<PortfolioCheck>();
                
                if (!checks.Any())
                {
                    _logger.LogWarning("Can't do {job}. PortfolioChecks not found", nameof(ExecuteRuleSetsJob));
                }

                foreach (var ruleSet in ruleSets)
                {
                    foreach (var rule in ruleSet.Rules)
                    {
                        var isActiveChanged = rule.Execute(checks);
                        
                        if (rule.IsNeedNotification(isActiveChanged))
                        {
                            await _notificationPublisher.PublishAsync(new MonitoringNotificationMessage
                            {
                                ChannelId = rule.NotificationChannelId,
                                Text = rule.GetNotificationMessage(checks)
                            });
                            rule.SetNotificationDate(DateTime.UtcNow);
                        }
                    }

                    await _ruleSetsStorage.AddOrUpdateAsync(ruleSet);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed {job}", nameof(ExecuteRuleSetsJob));
            }
        }
    }
}