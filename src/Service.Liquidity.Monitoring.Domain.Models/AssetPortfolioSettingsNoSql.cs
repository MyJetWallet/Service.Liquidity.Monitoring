
using MyNoSqlServer.Abstractions;

namespace Service.Liquidity.Monitoring.Domain.Models
{
    public class AssetPortfolioSettingsNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquidity-tradingportfolio-alarmsettings";
        public const string DefaultSettingsAsset = "_DEFAULT_";
        public const string TotalSettingsAsset = "_TOTAL_";
        public const string TotalNegativeInUsdSettingsAsset = "_TOTAL_NEGATIVE_USD_";
        public const string TotalNegativeInPercentSettingsAsset = "_TOTAL_NEGATIVE_PERCENT_";
        private static string GeneratePartitionKey(string asset) => $"{asset}";
        private static string GenerateRowKey() => "settings";
        
        public AssetPortfolioSettings Settings { get; set; }
        
        public static AssetPortfolioSettingsNoSql Create(AssetPortfolioSettings settings)
        {
            return new AssetPortfolioSettingsNoSql()
            {
                PartitionKey = GeneratePartitionKey(settings.Asset),
                RowKey = GenerateRowKey(),
                Settings = settings
            };
        }
    }
}
