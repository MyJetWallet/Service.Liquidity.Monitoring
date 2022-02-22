using MyNoSqlServer.Abstractions;

namespace Service.Liquidity.Monitoring.Domain.Models
{
    public class AssetPortfolioStatusNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquidity-tradingportfolio-alarmstatus";
        public const string TotalName = "_TOTAL_";
        public const string TotalNegativeInUsd = "_TOTAL_NEGATIVE_USD_";
        public const string TotalNegativeInPercent = "_TOTAL_NEGATIVE_PERCENT_";
        private static string GeneratePartitionKey(string asset) => $"{asset}";
        private static string GenerateRowKey() => "status";
        public AssetPortfolioStatus AssetStatus { get; set; }
        
        public static AssetPortfolioStatusNoSql Create(AssetPortfolioStatus assetPortfolioStatus)
        {
            return new AssetPortfolioStatusNoSql()
            {
                PartitionKey = GeneratePartitionKey(assetPortfolioStatus.Asset),
                RowKey = GenerateRowKey(),
                AssetStatus = assetPortfolioStatus
            };
        }
    }
}
