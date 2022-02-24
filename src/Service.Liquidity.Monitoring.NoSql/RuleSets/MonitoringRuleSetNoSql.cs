using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;

namespace Service.Liquidity.Monitoring.NoSql.RuleSets
{
    public class MonitoringRuleSetNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquidity-tradingportfolio-rulesets";
        public static string GeneratePartitionKey() => "*";
        public static string GenerateRowKey(string id) => id;

        public string Name { get; set; }
        public IEnumerable<MonitoringRule> Rules { get; set; }

        public static MonitoringRuleSetNoSql Create(MonitoringRuleSet src)
        {
            return new MonitoringRuleSetNoSql
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(src.Id),
                Name = src.Name,
                Rules = src.Rules
            };
        }
    }
}