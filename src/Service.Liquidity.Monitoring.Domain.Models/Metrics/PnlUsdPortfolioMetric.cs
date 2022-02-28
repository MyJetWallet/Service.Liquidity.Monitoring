using System.Linq;
using Service.Liquidity.Monitoring.Domain.Models.Metrics.Common;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Models.Metrics
{
    public class PnlUsdPortfolioMetric : IPortfolioMetric
    {
        public PortfolioMetricType Type { get; set; } = PortfolioMetricType.PnlUsd;

        public decimal Calculate(Portfolio portfolio, PortfolioMetricParams portfolioMetricParams)
        {
            var assetsValue = portfolio.Assets
                .Where(a => portfolioMetricParams.AssetSymbols.Contains(a.Key))
                .Sum(x => x.Value.NetBalanceInUsd);
            var compareAssetsValue = portfolio.Assets
                .Where(a => portfolioMetricParams.CompareAssetSymbols.Contains(a.Key))
                .Sum(x => x.Value.NetBalanceInUsd);
                    
            return assetsValue - compareAssetsValue;
        }
    }
}