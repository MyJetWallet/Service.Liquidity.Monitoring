using System.Collections.Generic;
using Service.Liquidity.Monitoring.Domain.Models.Metrics;
using Service.Liquidity.Monitoring.Domain.Models.Metrics.Common;

namespace Service.Liquidity.Monitoring.Domain.Services
{
    public interface IPortfolioMetricsFactory
    {
        IPortfolioMetric Get(PortfolioMetricType type);
        IEnumerable<IPortfolioMetric> Get();
    }
}