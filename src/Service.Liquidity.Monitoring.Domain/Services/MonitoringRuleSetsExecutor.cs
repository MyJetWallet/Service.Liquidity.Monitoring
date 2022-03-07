using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models;
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

        public MonitoringRuleSetsExecutor(
            ILogger<MonitoringRuleSetsExecutor> logger,
            IMonitoringRuleSetsStorage ruleSetsStorage,
            IMonitoringRuleSetsCache monitoringRuleSetsCache,
            IServiceBusPublisher<MonitoringNotificationMessage> notificationPublisher,
            IHedgeStrategiesFactory hedgeStrategiesFactory
        )
        {
            _logger = logger;
            _ruleSetsStorage = ruleSetsStorage;
            _monitoringRuleSetsCache = monitoringRuleSetsCache;
            _notificationPublisher = notificationPublisher;
            _hedgeStrategiesFactory = hedgeStrategiesFactory;
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
            var orderedRules = GetOrderedRules(ruleSet, checks, portfolio);

            foreach (var (hedgeCommandParams, rule) in orderedRules)
            {
                var isActiveChanged = rule.Execute(checks);

                if (rule.IsNeedNotification(isActiveChanged))
                {
                    var message = new MonitoringNotificationMessage
                    {
                        ChannelId = rule.NotificationChannelId,
                        Text = rule.GetNotificationText(checks)
                    };
                    await _notificationPublisher.PublishAsync(message);
                    _logger.LogInformation("Send Notification {@model}", message);
                    rule.SetNotificationSendDate(DateTime.UtcNow);
                }

                if (rule.HedgeStrategyType == HedgeStrategyType.Return)
                {
                    _logger.LogWarning($"RuleSet is skipped. Found Rule {rule.Name} with Return.");
                    return;
                }

                // do hedging
            }

            await _ruleSetsStorage.AddOrUpdateAsync(ruleSet);
        }

        private IEnumerable<(HedgeCommandParams hedgeCommandParams, MonitoringRule monitorinRule)> GetOrderedRules(
            MonitoringRuleSet ruleSet, IEnumerable<PortfolioCheck> checks, Portfolio portfolio)
        {
            var rules = new List<MonitoringRule>();

            foreach (var rule in ruleSet.Rules)
            {
                if (rule.HedgeStrategyType == HedgeStrategyType.Return)
                {
                    rules.Insert(0, rule);
                }
            }

            return rules
                .Select(rule =>
                {
                    var strategy = _hedgeStrategiesFactory.Get(rule.HedgeStrategyType);
                    var ruleChecks = checks.Where(ch => rule.CheckIds.Contains(ch.Id));
                    var commandParams =
                        strategy.GetCommandParams(portfolio, ruleChecks, rule.HedgeStrategyParams);

                    return (commandParams, rule);
                })
                .OrderByDescending(r => r.commandParams.Amount)
                .ToArray();
        }
    }
}