using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
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
                await RefreshAndSendNotificationsAsync(ruleSet, checksArr, portfolio);
            }

            await HedgeAsync(ruleSets);
        }

        private async Task RefreshAndSendNotificationsAsync(MonitoringRuleSet ruleSet, PortfolioCheck[] checks,
            Portfolio portfolio)
        {
            foreach (var rule in ruleSet.Rules)
            {
                var strategy = _hedgeStrategiesFactory.Get(rule.HedgeStrategyType);
                rule.RefreshState(portfolio, checks, strategy);

                if (rule.NeedsNotification())
                {
                    await _notificationPublisher.PublishAsync(new MonitoringNotificationMessage
                    {
                        ChannelId = rule.NotificationChannelId,
                        Text = rule.GetNotificationText(checks)
                    });
                    rule.SetNotificationSendDate(DateTime.UtcNow);
                }
            }

            await _ruleSetsStorage.AddOrUpdateAsync(ruleSet);
        }

        private async Task HedgeAsync(List<MonitoringRuleSet> ruleSets)
        {
            var hightestPriorityRule = ruleSets
                .Where(rs => rs.NeedsHedging())
                .SelectMany(rs => rs.Rules)
                .Where(rule => rule.CurrentState.HedgeParams.Validate(out _))
                .MaxBy(r => r.CurrentState.HedgeParams.BuyAmount);

            if (hightestPriorityRule == null)
            {
                _logger.LogWarning("No rule for hedging");
                return;
            }

            if (!hightestPriorityRule.CurrentState.HedgeParams.Validate(out var errors))
            {
                _logger.LogWarning(
                    $"Hedging is skipped. Found Rule {hightestPriorityRule.Name} with invalid HedgeParams {string.Join(", ", errors)}");
                return;
            }

            await _hedgeService.HedgeAsync(hightestPriorityRule.CurrentState.HedgeParams);
        }
    }
}