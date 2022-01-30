using MyNoSqlServer.Abstractions;

namespace Service.Liquidity.Monitoring.Domain.Models
{
    public class AssetPortfolioVelocityStatusNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquidity-tradingportfolio-velocity";
        private static string GeneratePartitionKey(string asset) => $"asset:{asset}";
        private static string GenerateRowKey() => "velocity";
        public AssetPortfolioStatus AssetStatus { get; set; }
        
        public static AssetPortfolioVelocityStatusNoSql Create(AssetPortfolioStatus assetPortfolioStatus)
        {
            return new AssetPortfolioVelocityStatusNoSql()
            {
                PartitionKey = GeneratePartitionKey(assetPortfolioStatus.Asset),
                RowKey = GenerateRowKey(),
                AssetStatus = assetPortfolioStatus
            };
        }
    }
}
