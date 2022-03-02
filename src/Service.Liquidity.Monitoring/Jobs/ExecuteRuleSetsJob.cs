﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using MyJetWallet.Sdk.ServiceBus;
using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.Metrics.Common;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;
using Service.Liquidity.Monitoring.Domain.Services;
using Service.Liquidity.TradingPortfolio.Domain.Models.NoSql;

namespace Service.Liquidity.Monitoring.Jobs
{
    public class ExecuteRuleSetsJob : IStartable
    {
        private readonly ILogger<ExecuteRuleSetsJob> _logger;
        private readonly IMonitoringRuleSetsStorage _ruleSetsStorage;
        private readonly IPortfolioChecksStorage _portfolioChecksStorage;
        private readonly IMyNoSqlServerDataReader<PortfolioNoSql> _portfolioReader;
        private readonly IPortfolioMetricsFactory _portfolioMetricsFactory;
        private readonly IServiceBusPublisher<MonitoringNotificationMessage> _notificationPublisher;
        private readonly MyTaskTimer _timer;

        public ExecuteRuleSetsJob(
            ILogger<ExecuteRuleSetsJob> logger,
            IMonitoringRuleSetsStorage ruleSetsStorage,
            IPortfolioChecksStorage portfolioChecksStorage,
            IMyNoSqlServerDataReader<PortfolioNoSql> portfolioReader,
            IPortfolioMetricsFactory portfolioMetricsFactory,
            IServiceBusPublisher<MonitoringNotificationMessage> notificationPublisher
        )
        {
            _logger = logger;
            _ruleSetsStorage = ruleSetsStorage;
            _portfolioChecksStorage = portfolioChecksStorage;
            _portfolioReader = portfolioReader;
            _portfolioMetricsFactory = portfolioMetricsFactory;
            _notificationPublisher = notificationPublisher;
            _timer = new MyTaskTimer(nameof(ExecuteRuleSetsJob),
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
                var checks = await ExecutePortfolioChecksAsync();
                await ExecuteRuleSetsAsync(checks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{job} failed", nameof(ExecuteRuleSetsJob));
            }
        }

        private async Task<IEnumerable<PortfolioCheck>> ExecutePortfolioChecksAsync()
        {
            var portfolio = _portfolioReader.Get().FirstOrDefault()?.Portfolio;

            if (portfolio == null)
            {
                _logger.LogWarning("Can't ExecutePortfolioChecksAsync. Portfolio Not found");

                return ArraySegment<PortfolioCheck>.Empty;
            }

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

        private async Task ExecuteRuleSetsAsync(IEnumerable<PortfolioCheck> checks)
        {
            var checksArr = checks?.ToArray() ?? ArraySegment<PortfolioCheck>.Empty;

            if (!checksArr.Any())
            {
                _logger.LogWarning("Can't ExecuteRuleSetsAsync. PortfolioChecks not found");
            }

            var ruleSets = (await _ruleSetsStorage.GetAsync())?.ToList() ?? new List<MonitoringRuleSet>();

            if (!ruleSets.Any())
            {
                _logger.LogWarning("Can't ExecuteRuleSetsAsync. RuleSets not found");
            }

            foreach (var ruleSet in ruleSets)
            {
                foreach (var rule in ruleSet.Rules)
                {
                    var isActiveChanged = rule.Execute(checksArr);

                    if (rule.IsNeedNotification(isActiveChanged))
                    {
                        await _notificationPublisher.PublishAsync(new MonitoringNotificationMessage
                        {
                            ChannelId = rule.NotificationChannelId,
                            Text = rule.GetNotificationMessage(checksArr)
                        });
                        rule.SetNotificationSendDate(DateTime.UtcNow);
                    }
                }

                await _ruleSetsStorage.AddOrUpdateAsync(ruleSet);
            }
        }
    }
}