using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.Checks.Operators;
using Service.Liquidity.Monitoring.Domain.Models.Checks.Strategies;

namespace Service.Liquidity.Monitoring.NoSql.Checks
{
    public class PortfolioCheckNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquidity-tradingportfolio-checks";
        public static string GeneratePartitionKey() => "*";
        public static string GenerateRowKey(string id) => id;

        public string Name { get; set; }
        public IEnumerable<string> AssetSymbols { get; set; }
        public IEnumerable<string> CompareAssetSymbols { get; set; }
        public decimal TargetValue { get; set; }
        public PortfolioCheckStrategyType StrategyType { get; set; }
        public CheckOperatorType OperatorType { get; set; }
        public PortfolioCheckState CurrentState { get; set; }
        public PortfolioCheckState PrevState { get; set; }
        public static PortfolioCheckNoSql Create(PortfolioCheck src)
        {
            return new PortfolioCheckNoSql
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(src.Id),
                Name = src.Name,
                AssetSymbols = src.AssetSymbols,
                CompareAssetSymbols = src.CompareAssetSymbols,
                CurrentState = src.CurrentState,
                OperatorType = src.OperatorType,
                PrevState = src.PrevState,
                StrategyType = src.StrategyType,
                TargetValue = src.TargetValue
            };
        }
    }
}