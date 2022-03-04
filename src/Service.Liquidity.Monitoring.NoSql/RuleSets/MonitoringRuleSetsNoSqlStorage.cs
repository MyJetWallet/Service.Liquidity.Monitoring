using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;
using Service.Liquidity.Monitoring.Domain.Services;

namespace Service.Liquidity.Monitoring.NoSql.RuleSets
{
    public class MonitoringRuleSetsNoSqlStorage : IMonitoringRuleSetsStorage
    {
        private readonly IMyNoSqlServerDataWriter<MonitoringRuleSetNoSql> _myNoSqlServerDataWriter;
        private Dictionary<string, MonitoringRuleSet> _ruleSetsCache = new ();
        
        public MonitoringRuleSetsNoSqlStorage(
            IMyNoSqlServerDataWriter<MonitoringRuleSetNoSql> myNoSqlServerDataWriter
        )
        {
            _myNoSqlServerDataWriter = myNoSqlServerDataWriter;
        }

        public async Task AddOrUpdateAsync(MonitoringRuleSet model)
        {
            var nosqlModel = MonitoringRuleSetNoSql.Create(model);
            await _myNoSqlServerDataWriter.InsertOrReplaceAsync(nosqlModel);
            _ruleSetsCache[model.Id] = model;
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