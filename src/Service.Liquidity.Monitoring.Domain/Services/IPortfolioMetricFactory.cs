using Service.Liquidity.Monitoring.Domain.Models.Metrics;
using Service.Liquidity.Monitoring.Domain.Models.Metrics.Common;

namespace Service.Liquidity.Monitoring.Domain.Services
{
    public interface IPortfolioMetricFactory
    {
        IPortfolioMetric Get(PortfolioMetricType type);
    }
}