using System.Linq;
using Service.Liquidity.Monitoring.Domain.Models.Metrics.Common;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Models.Metrics
{
    public class CollateralUsdPortfolioMetric : IPortfolioMetric
    {
        public PortfolioMetricType Type { get; set; } = PortfolioMetricType.CollateralUsd;

        public decimal Calculate(Portfolio portfolio, PortfolioMetricParams portfolioMetricParams)
        {
            var assetsValue = portfolio.Assets
                .Where(a => portfolioMetricParams.AssetSymbols.Contains(a.Key))
                .Sum(x => x.Value.GetPositiveNetInUsd());
            var compareAssetsValue = portfolio.Assets
                .Where(a => portfolioMetricParams.CompareAssetSymbols.Contains(a.Key))
                .Sum(x => x.Value.GetPositiveNetInUsd());
                    
            return assetsValue - compareAssetsValue;
        }
    }
}