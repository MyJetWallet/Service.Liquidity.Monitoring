using System.Collections.Generic;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.Domain.Interfaces;

public interface IMonitoringRuleSetsCache
{
    IEnumerable<MonitoringRuleSet> Get();
}