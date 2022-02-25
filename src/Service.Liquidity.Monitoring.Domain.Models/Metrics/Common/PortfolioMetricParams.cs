using System.Collections.Generic;

namespace Service.Liquidity.Monitoring.Domain.Models.Metrics.Common
{
    public class PortfolioMetricParams
    {
        public IEnumerable<string> AssetSymbols { get; set; }
        public IEnumerable<string> CompareAssetSymbols { get; set; }
    }
}