using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.NoSql.Rules
{
    public class MonitoringRulesNoSqlStorage : IMonitoringRulesStorage
    {
        private readonly IMyNoSqlServerDataWriter<MonitoringRuleNoSql> _myNoSqlServerDataWriter;
        
        public MonitoringRulesNoSqlStorage(
            IMyNoSqlServerDataWriter<MonitoringRuleNoSql> myNoSqlServerDataWriter
        )
        {
            _myNoSqlServerDataWriter = myNoSqlServerDataWriter;
        }

        public async Task AddOrUpdateAsync(MonitoringRule model)
        {
            var nosqlModel = MonitoringRuleNoSql.Create(model);
            await _myNoSqlServerDataWriter.InsertOrReplaceAsync(nosqlModel);
        }

        public async Task<IEnumerable<MonitoringRule>> GetAsync()
        {
            var models = await _myNoSqlServerDataWriter.GetAsync();

            return models?.Select(m => m.Value) ?? new List<MonitoringRule>();
        }

        public async Task<MonitoringRule> GetAsync(string id)
        {
            var model = await _myNoSqlServerDataWriter.GetAsync(
                MonitoringRuleNoSql.GeneratePartitionKey(),
                MonitoringRuleNoSql.GenerateRowKey(id));

            return model?.Value;
        }

        public async Task DeleteAsync(string id)
        {
            await _myNoSqlServerDataWriter.DeleteAsync(MonitoringRuleNoSql.GeneratePartitionKey(),
                MonitoringRuleNoSql.GenerateRowKey(id));
        }

        public async Task AddOrUpdateAsync(IEnumerable<MonitoringRule> models)
        {
            var nosqlModels = models.Select(MonitoringRuleNoSql.Create);
            await _myNoSqlServerDataWriter.BulkInsertOrReplaceAsync(nosqlModels);
        }
        
        public async Task UpdateStatesAsync(IEnumerable<MonitoringRule> models)
        {
            var dbModels = (await _myNoSqlServerDataWriter.GetAsync())?.ToArray();
            var updatedModels = models.ToDictionary(x => x.Id);

            if (dbModels == null)
            {
                return;
            }

            foreach (var dbModel in dbModels)
            {
                if (updatedModels.TryGetValue(dbModel.Value.Id, out var updatedModel))
                {
                    dbModel.Value.CurrentState = updatedModel?.CurrentState;
                    dbModel.Value.PrevState = updatedModel?.PrevState;

                    foreach (var dbCheck in dbModel.Value.Checks ?? new List<PortfolioCheck>())
                    {
                        dbCheck.CurrentState = updatedModel?.Checks
                            .FirstOrDefault(ch => ch.Id == dbCheck.Id)?.CurrentState;
                        dbCheck.PrevState = updatedModel?.Checks
                            .FirstOrDefault(ch => ch.Id == dbCheck.Id)?.PrevState;
                    }
                }
            }
            
            await _myNoSqlServerDataWriter.BulkInsertOrReplaceAsync(dbModels);
        }
    }
}