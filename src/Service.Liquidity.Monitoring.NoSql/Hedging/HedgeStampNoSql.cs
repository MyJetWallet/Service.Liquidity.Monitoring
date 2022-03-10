using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Models.Hedging;

namespace Service.Liquidity.Monitoring.NoSql.Hedging;

public class HedgeStampNoSql : MyNoSqlDbEntity
{
    public const string TableName = "myjetwallet-liquidity-hedgestamp";
    public static string GeneratePartitionKey() => "*";
    public static string GenerateRowKey() => "*";

    public HedgeStamp Value { get; set; }

    public static HedgeStampNoSql Create(HedgeStamp src)
    {
        return new HedgeStampNoSql
        {
            PartitionKey = GeneratePartitionKey(),
            RowKey = GenerateRowKey(),
            Value = src
        };
    }
}