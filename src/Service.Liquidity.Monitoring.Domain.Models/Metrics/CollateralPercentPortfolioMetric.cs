using System.Linq;
using Service.Liquidity.Monitoring.Domain.Models.Metrics.Common;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Models.Metrics
{
    public class CollateralPercentPortfolioMetric : IPortfolioMetric
    {
        public PortfolioMetricType Type { get; set; } = PortfolioMetricType.CollateralPercent;

        public decimal Calculate(Portfolio portfolio, PortfolioMetricParams portfolioMetricParams)
        {
            var assetsValue = portfolio.Assets
                .Where(a => portfolioMetricParams.AssetSymbols.Contains(a.Key))
                .Sum(x => x.Value.GetPositiveNetInUsd());
            var assetsValuePercent = assetsValue / portfolio.TotalPositiveNetInUsd * 100;
            
            var compareAssetsValue = portfolio.Assets
                .Where(a => portfolioMetricParams.CompareAssetSymbols.Contains(a.Key))
                .Sum(x => x.Value.GetPositiveNetInUsd());
            var compareAssetsValuePercent = compareAssetsValue / portfolio.TotalPositiveNetInUsd * 100;

            return assetsValuePercent - compareAssetsValuePercent;
        }
    }
}