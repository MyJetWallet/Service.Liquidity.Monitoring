using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.NoSql.Backups;

public class MonitoringRulesBackupInfoNoSql : MyNoSqlDbEntity
{
    public const string TableName = "myjetwallet-liquidity-monitoring-rules-backup-infos";
    public static string GeneratePartitionKey() => "*";
    public static string GenerateRowKey(string id) => id;

    public MonitoringRulesBackupInfo Value { get; set; }

    public static MonitoringRulesBackupInfoNoSql Create(MonitoringRulesBackup src)
    {
        return new MonitoringRulesBackupInfoNoSql
        {
            PartitionKey = GeneratePartitionKey(),
            RowKey = GenerateRowKey(src.Id),
            Value = new MonitoringRulesBackupInfo
            {
                BackupId = src.Id,
                BackupName = src.Name,
                BackupCreatedBy = src.CreatedBy,
                BackupCreatedDate = src.CreatedDate
            }
        };
    }
}