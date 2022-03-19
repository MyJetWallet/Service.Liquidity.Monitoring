using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;

namespace Service.Liquidity.Monitoring.NoSql.Rules
{
    public class MonitoringRuleNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquidity-moniotring-rules";
        public static string GeneratePartitionKey() => "*";
        public static string GenerateRowKey(string id) => id;

        public MonitoringRule Value { get; set; }

        public static MonitoringRuleNoSql Create(MonitoringRule src)
        {
            return new MonitoringRuleNoSql
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(src.Id),
                Value = src
            };
        }
    }
}