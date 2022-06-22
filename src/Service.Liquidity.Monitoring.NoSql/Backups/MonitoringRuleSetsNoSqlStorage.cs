using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.NoSql.Backups
{
    public class MonitoringRulesBackupsNoSqlStorage : IMonitoringRulesBackupsStorage
    {
        private readonly IMyNoSqlServerDataWriter<MonitoringRulesBackupNoSql> _backupsWriter;
        private readonly IMyNoSqlServerDataWriter<MonitoringRulesBackupInfoNoSql> _infosWriter;

        public MonitoringRulesBackupsNoSqlStorage(
            IMyNoSqlServerDataWriter<MonitoringRulesBackupNoSql> backupsWriter,
            IMyNoSqlServerDataWriter<MonitoringRulesBackupInfoNoSql> infosWriter
        )
        {
            _backupsWriter = backupsWriter;
            _infosWriter = infosWriter;
        }

        public async Task AddOrUpdateAsync(MonitoringRulesBackup model)
        {
            foreach (var rule in model.MonitoringRules ?? new List<MonitoringRule>())
            {
                rule.CurrentState = null;
                rule.PrevState = null;

                foreach (var check in rule.Checks ?? new List<PortfolioCheck>())
                {
                    check.PrevState = null;
                    check.CurrentState = null;
                }
            }
            
            var nosqlModel = MonitoringRulesBackupNoSql.Create(model);
            var existingBackup = await _backupsWriter.GetAsync(nosqlModel.PartitionKey, nosqlModel.RowKey);
            
            if (existingBackup == null)
            {
                var infoNoSql = MonitoringRulesBackupInfoNoSql.Create(model);
                await _infosWriter.InsertOrReplaceAsync(infoNoSql);
            }
            
            await _backupsWriter.InsertOrReplaceAsync(nosqlModel);
        }

        public async Task<IEnumerable<MonitoringRulesBackupInfo>> GetInfosAsync()
        {
            var models = await _infosWriter.GetAsync();

            return models?.Select(m => m.Value) ?? new List<MonitoringRulesBackupInfo>();
        }

        public async Task<MonitoringRulesBackup> GetAsync(string id)
        {
            var model = await _backupsWriter.GetAsync(
                MonitoringRulesBackupNoSql.GeneratePartitionKey(id),
                MonitoringRulesBackupNoSql.GenerateRowKey(id));

            return model?.Value;
        }

        public async Task DeleteAsync(string id)
        {
            await _backupsWriter.DeleteAsync(
                MonitoringRulesBackupNoSql.GeneratePartitionKey(id),
                MonitoringRulesBackupNoSql.GenerateRowKey(id));
        }
    }
}