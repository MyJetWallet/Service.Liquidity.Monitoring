using System.Collections.Generic;

namespace Service.Liquidity.Monitoring.Domain.Models.Checks.Strategies
{
    public class PortfolioCheckStrategyParams
    {
        public IEnumerable<string> AssetSymbols { get; set; }
        public IEnumerable<string> CompareAssetSymbols { get; set; }
        public decimal TargetValue { get; set; }
    }
}