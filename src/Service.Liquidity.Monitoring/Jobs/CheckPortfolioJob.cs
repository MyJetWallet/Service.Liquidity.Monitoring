﻿using System;
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
        private readonly IMyNoSqlServerDataReader<PortfolioNoSql> _myNoSqlServerDataReader;
        private readonly MyTaskTimer _timer;

        public CheckPortfolioJob(
            ILogger<CheckPortfolioJob> logger,
            IPortfolioChecksExecutor portfolioChecksExecutor,
            IMonitoringRuleSetsExecutor monitoringRuleSetsExecutor,
            IServiceBusPublisher<PortfolioMonitoringMessage> publisher,
            IManualInputService portfolioService,
            IMyNoSqlServerDataReader<PortfolioNoSql> myNoSqlServerDataReader
        )
        {
            _logger = logger;
            _portfolioChecksExecutor = portfolioChecksExecutor;
            _monitoringRuleSetsExecutor = monitoringRuleSetsExecutor;
            _publisher = publisher;
            _portfolioService = portfolioService;
            _myNoSqlServerDataReader = myNoSqlServerDataReader;
            _timer = new MyTaskTimer(nameof(CheckPortfolioJob),
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
                var portfolio = _myNoSqlServerDataReader.Get().FirstOrDefault()?.Portfolio;

                if (portfolio == null)
                {
                    _logger.LogWarning($"Can't do {nameof(CheckPortfolioJob)}. Portfolio Not found");
                    return;
                }

                var checks = await _portfolioChecksExecutor.ExecuteAsync(portfolio);

                if (checks == null || !checks.Any())
                {
                    _logger.LogWarning($"Can't do {nameof(CheckPortfolioJob)}. Checks Not found");
                    return;
                }

                var ruleSets = await _monitoringRuleSetsExecutor.ExecuteAsync(portfolio, checks);

                if (ruleSets == null || !ruleSets.Any())
                {
                    _logger.LogWarning($"Can't do {nameof(CheckPortfolioJob)}. RuleSets Not found");
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
                _logger.LogError(ex, "{job} failed", nameof(CheckPortfolioJob));
            }
        }
    }
}