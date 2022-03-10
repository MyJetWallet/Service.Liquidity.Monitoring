using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models.Hedging;

namespace Service.Liquidity.Monitoring.NoSql.Hedging;

public class HedgeStampNoSqlStorage : IHedgeStampStorage
{
    private readonly IMyNoSqlServerDataWriter<HedgeStampNoSql> _myNoSqlServerDataWriter;

    public HedgeStampNoSqlStorage(
        IMyNoSqlServerDataWriter<HedgeStampNoSql> myNoSqlServerDataWriter
    )
    {
        _myNoSqlServerDataWriter = myNoSqlServerDataWriter;
    }

    public async Task AddOrUpdateAsync(HedgeStamp model)
    {
        var nosqlModel = HedgeStampNoSql.Create(model);
        await _myNoSqlServerDataWriter.InsertOrReplaceAsync(nosqlModel);
    }

    public async Task<HedgeStamp> GetAsync()
    {
        var model = await _myNoSqlServerDataWriter.GetAsync(HedgeStampNoSql.GeneratePartitionKey(),
            HedgeStampNoSql.GenerateRowKey());

        return model?.Value;
    }
}