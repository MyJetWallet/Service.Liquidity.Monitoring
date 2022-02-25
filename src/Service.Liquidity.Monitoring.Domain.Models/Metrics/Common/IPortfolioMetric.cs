using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Models.Metrics.Common
{
    public interface IPortfolioMetric
    {
        public PortfolioMetricType Type { get; set; }

        public decimal Calculate(Portfolio portfolio, PortfolioMetricParams portfolioMetricParams);
    }
}