using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

        public MonitoringRuleSetsExecutor(
            ILogger<MonitoringRuleSetsExecutor> logger,
            IMonitoringRuleSetsStorage ruleSetsStorage,
            IMonitoringRuleSetsCache monitoringRuleSetsCache
        )
        {
            _logger = logger;
            _ruleSetsStorage = ruleSetsStorage;
            _monitoringRuleSetsCache = monitoringRuleSetsCache;
        }

        public async Task<ICollection<MonitoringRuleSet>> ExecuteAsync(Portfolio portfolio,
            IEnumerable<PortfolioCheck> checks)
        {
            var checksArr = checks?.ToArray() ?? Array.Empty<PortfolioCheck>();

            if (!checksArr.Any())
            {
                return Array.Empty<MonitoringRuleSet>();
            }

            var ruleSets = _monitoringRuleSetsCache.Get()?.ToList() ?? new List<MonitoringRuleSet>();

            if (!ruleSets.Any())
            {
                return Array.Empty<MonitoringRuleSet>();
            }

            foreach (var ruleSet in ruleSets)
            {
                await RefreshStateAsync(ruleSet, checksArr);
            }

            return ruleSets;
        }

        private async Task RefreshStateAsync(MonitoringRuleSet ruleSet, PortfolioCheck[] checks)
        {
            foreach (var rule in ruleSet.Rules ?? Array.Empty<MonitoringRule>())
            {
                rule.RefreshState(checks);
            }

            await _ruleSetsStorage.AddOrUpdateAsync(ruleSet);
        }
    }
}