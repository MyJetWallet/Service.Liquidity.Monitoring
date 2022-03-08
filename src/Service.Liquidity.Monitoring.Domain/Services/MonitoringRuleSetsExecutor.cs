using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.Hedging.Common;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Services
{
    public class MonitoringRuleSetsExecutor : IMonitoringRuleSetsExecutor
    {
        private readonly ILogger<MonitoringRuleSetsExecutor> _logger;
        private readonly IMonitoringRuleSetsStorage _ruleSetsStorage;
        private readonly IMonitoringRuleSetsCache _monitoringRuleSetsCache;
        private readonly IServiceBusPublisher<MonitoringNotificationMessage> _notificationPublisher;
        private readonly IHedgeStrategiesFactory _hedgeStrategiesFactory;
        private readonly IHedgeService _hedgeService;

        public MonitoringRuleSetsExecutor(
            ILogger<MonitoringRuleSetsExecutor> logger,
            IMonitoringRuleSetsStorage ruleSetsStorage,
            IMonitoringRuleSetsCache monitoringRuleSetsCache,
            IServiceBusPublisher<MonitoringNotificationMessage> notificationPublisher,
            IHedgeStrategiesFactory hedgeStrategiesFactory,
            IHedgeService hedgeService
        )
        {
            _logger = logger;
            _ruleSetsStorage = ruleSetsStorage;
            _monitoringRuleSetsCache = monitoringRuleSetsCache;
            _notificationPublisher = notificationPublisher;
            _hedgeStrategiesFactory = hedgeStrategiesFactory;
            _hedgeService = hedgeService;
        }

        public async Task ExecuteAsync(Portfolio portfolio, IEnumerable<PortfolioCheck> checks)
        {
            var checksArr = checks?.ToArray() ?? Array.Empty<PortfolioCheck>();

            if (!checksArr.Any())
            {
                _logger.LogWarning("Can't ExecuteRuleSetsAsync. PortfolioChecks not found");
                return;
            }

            var ruleSets = _monitoringRuleSetsCache.Get()?.ToList() ?? new List<MonitoringRuleSet>();

            if (!ruleSets.Any())
            {
                _logger.LogWarning("Can't ExecuteRuleSetsAsync. RuleSets not found");
                return;
            }

            foreach (var ruleSet in ruleSets)
            {
                await ExecuteRuleSetAsync(ruleSet, checksArr, portfolio);
            }
        }

        private async Task ExecuteRuleSetAsync(MonitoringRuleSet ruleSet, PortfolioCheck[] checks, Portfolio portfolio)
        {
            foreach (var rule in ruleSet.Rules)
            {
                var strategy = _hedgeStrategiesFactory.Get(rule.HedgeStrategyType);
                rule.RefreshState(portfolio, checks, strategy);
            }
            
            ruleSet.OrderRules();

            foreach (var rule in ruleSet.Rules)
            {
                if (rule.IsNeedNotification())
                {
                    await _notificationPublisher.PublishAsync(new MonitoringNotificationMessage
                    {
                        ChannelId = rule.NotificationChannelId,
                        Text = rule.GetNotificationText(checks)
                    });
                    rule.SetNotificationSendDate(DateTime.UtcNow);
                }

                if (rule.HedgeStrategyType == HedgeStrategyType.Return)
                {
                    _logger.LogWarning($"RuleSet is skipped. Found Rule {rule.Name} with ReturnStrategy");
                    return;
                }

                if (rule.HedgeStrategyType == HedgeStrategyType.None)
                {
                    _logger.LogWarning($"Hedging is skipped. Found Rule {rule.Name} with NoneStrategy");
                    return;
                }
                
                await _hedgeService.HedgeAsync(rule.CurrentState.HedgeParams);

            }

            await _ruleSetsStorage.AddOrUpdateAsync(ruleSet);
        }
    }
}