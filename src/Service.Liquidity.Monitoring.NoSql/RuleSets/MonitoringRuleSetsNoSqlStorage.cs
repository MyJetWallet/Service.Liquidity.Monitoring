using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;

namespace Service.Liquidity.Monitoring.NoSql.RuleSets
{
    public class MonitoringRuleSetsNoSqlStorage : IMonitoringRuleSetsStorage
    {
        private readonly IMyNoSqlServerDataWriter<MonitoringRuleSetNoSql> _myNoSqlServerDataWriter;
        
        public MonitoringRuleSetsNoSqlStorage(
            IMyNoSqlServerDataWriter<MonitoringRuleSetNoSql> myNoSqlServerDataWriter
        )
        {
            _myNoSqlServerDataWriter = myNoSqlServerDataWriter;
        }
        
        public async Task UpdateRuleStatesAsync(MonitoringRuleSet model)
        {
            var dbModel = await GetAsync(model.Id);

            if (dbModel?.Rules == null)
            {
                return;
            }

            foreach (var dbRule in dbModel.Rules)
            {
                var updatedRule = model.Rules.FirstOrDefault(r => r.Id == dbRule.Id);
                dbRule.CurrentState = updatedRule?.CurrentState;
                dbRule.PrevState = updatedRule?.PrevState;
            }
            
            var nosqlModel = MonitoringRuleSetNoSql.Create(dbModel);
            await _myNoSqlServerDataWriter.InsertOrReplaceAsync(nosqlModel);
        }

        public async Task AddOrUpdateAsync(MonitoringRuleSet model)
        {
            var nosqlModel = MonitoringRuleSetNoSql.Create(model);
            await _myNoSqlServerDataWriter.InsertOrReplaceAsync(nosqlModel);
        }

        public async Task<IEnumerable<MonitoringRuleSet>> GetAsync()
        {
            var models = await _myNoSqlServerDataWriter.GetAsync();

            return models.Select(m => m.Value);
        }

        public async Task<MonitoringRuleSet> GetAsync(string id)
        {
            var model = await _myNoSqlServerDataWriter.GetAsync(MonitoringRuleSetNoSql.GeneratePartitionKey(),
                MonitoringRuleSetNoSql.GenerateRowKey(id));

            return model.Value;
        }

        public async Task DeleteAsync(string id)
        {
            await _myNoSqlServerDataWriter.DeleteAsync(MonitoringRuleSetNoSql.GeneratePartitionKey(),
                MonitoringRuleSetNoSql.GenerateRowKey(id));
        }
    }
}