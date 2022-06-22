using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.Domain.Interfaces;

public interface IMonitoringRulesBackupsStorage
{
    Task<IEnumerable<MonitoringRulesBackupInfo>> GetInfosAsync();
    Task AddOrUpdateAsync(MonitoringRulesBackup model);
    Task<MonitoringRulesBackup> GetAsync(string id);
    Task DeleteAsync(string id);
}