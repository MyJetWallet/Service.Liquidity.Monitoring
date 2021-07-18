using MyNoSqlServer.Abstractions;

namespace Service.Liquidity.Monitoring.Domain.Models
{
    public class AssetPortfolioStatusNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-liquidity-monitoring-assetStatus";
        private static string GeneratePartitionKey(string asset) => $"asset:{asset}";
        private static string GenerateRowKey() => "status";
        public AssetPortfolioStatus AssetStatus { get; set; }
        
        public static AssetPortfolioStatusNoSql Create(AssetPortfolioStatus assetPortfolioStatus)
        {
            return new()
            {
                PartitionKey = GeneratePartitionKey(assetPortfolioStatus.Asset),
                RowKey = GenerateRowKey(),
                AssetStatus = assetPortfolioStatus
            };
        }
    }
}
