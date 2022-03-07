using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Models.Checks;

namespace Service.Liquidity.Monitoring.NoSql.Checks
{
    public class PortfolioCheckNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquidity-tradingportfolio-checks";
        public static string GeneratePartitionKey() => "*";
        public static string GenerateRowKey(string id) => id;

        public PortfolioCheck Value { get; set; }

        public static PortfolioCheckNoSql Create(PortfolioCheck src)
        {
            return new PortfolioCheckNoSql
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(src.Id),
                Value = src
            };
        }
    }
}