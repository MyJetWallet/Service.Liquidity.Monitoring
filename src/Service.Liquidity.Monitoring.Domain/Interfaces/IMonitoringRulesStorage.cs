using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.Domain.Interfaces;

public interface IMonitoringRulesStorage
{
    Task<IEnumerable<MonitoringRule>> GetAsync();
    Task AddOrUpdateAsync(MonitoringRule model);
    Task<MonitoringRule> GetAsync(string id);
    Task DeleteAsync(string id);
    Task AddOrUpdateAsync(IEnumerable<MonitoringRule> models);
    Task UpdateStatesAsync(IEnumerable<MonitoringRule> models);
}