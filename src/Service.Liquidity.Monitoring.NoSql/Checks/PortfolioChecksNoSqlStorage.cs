using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Services;

namespace Service.Liquidity.Monitoring.NoSql.Checks
{
    public class PortfolioChecksNoSqlStorage : IPortfolioChecksStorage
    {
        private readonly IMyNoSqlServerDataWriter<PortfolioCheckNoSql> _myNoSqlServerDataWriter;

        public PortfolioChecksNoSqlStorage(
            IMyNoSqlServerDataWriter<PortfolioCheckNoSql> myNoSqlServerDataWriter
        )
        {
            _myNoSqlServerDataWriter = myNoSqlServerDataWriter;
        }

        public async Task AddOrUpdateAsync(PortfolioCheck model)
        {
            var nosqlModel = PortfolioCheckNoSql.Create(model);
            await _myNoSqlServerDataWriter.InsertOrReplaceAsync(nosqlModel);
        }

        public async Task<IEnumerable<PortfolioCheck>> GetAsync()
        {
            var models = await _myNoSqlServerDataWriter.GetAsync();

            return models.Select(m => m.Value);
        }

        public async Task<PortfolioCheck> GetAsync(string id)
        {
            var model = await _myNoSqlServerDataWriter.GetAsync(PortfolioCheckNoSql.GeneratePartitionKey(),
                PortfolioCheckNoSql.GenerateRowKey(id));

            return model.Value;
        }

        public async Task DeleteAsync(string id)
        {
            await _myNoSqlServerDataWriter.DeleteAsync(PortfolioCheckNoSql.GeneratePartitionKey(),
                PortfolioCheckNoSql.GenerateRowKey(id));
        }
        
        public async Task BulkInsetOrUpdateAsync(IEnumerable<PortfolioCheck> models)
        {
            var nosqlModels = models.Select(PortfolioCheckNoSql.Create);
            await _myNoSqlServerDataWriter.BulkInsertOrReplaceAsync(nosqlModels);
        }
    }
}