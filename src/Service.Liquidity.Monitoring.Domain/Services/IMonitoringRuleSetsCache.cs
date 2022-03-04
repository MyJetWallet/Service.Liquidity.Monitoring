﻿using System.Collections.Generic;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;

namespace Service.Liquidity.Monitoring.Domain.Services;

public interface IMonitoringRuleSetsCache
{
    IEnumerable<MonitoringRuleSet> Get();
}