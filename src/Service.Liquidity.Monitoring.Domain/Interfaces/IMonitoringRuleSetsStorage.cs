using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.Domain.Interfaces
{
    public interface IMonitoringRuleSetsStorage
    {
        Task<IEnumerable<MonitoringRuleSet>> GetAsync();
        Task AddOrUpdateAsync(MonitoringRuleSet model);
        Task<MonitoringRuleSet> GetAsync(string id);
        Task DeleteAsync(string id);
        Task UpdateRuleStatesAsync(MonitoringRuleSet model);
    }
}