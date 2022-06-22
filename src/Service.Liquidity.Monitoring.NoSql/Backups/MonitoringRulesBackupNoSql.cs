using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.NoSql.Backups;

public class MonitoringRulesBackupNoSql : MyNoSqlDbEntity
{
    public const string TableName = "myjetwallet-liquidity-monitoring-rules-backups";
    public static string GeneratePartitionKey(string id) => id;
    public static string GenerateRowKey(string id) => id;

    public MonitoringRulesBackup Value { get; set; }

    public static MonitoringRulesBackupNoSql Create(MonitoringRulesBackup src)
    {
        src.Id ??= Guid.NewGuid().ToString();
        return new MonitoringRulesBackupNoSql
        {
            PartitionKey = GeneratePartitionKey(src.Id),
            RowKey = GenerateRowKey(src.Id),
            Value = src
        };
    }
}