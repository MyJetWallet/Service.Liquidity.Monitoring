using System.Collections.Generic;
using Service.Liquidity.Monitoring.Domain.Models.Metrics.Common;

namespace Service.Liquidity.Monitoring.Domain.Interfaces
{
    public interface IPortfolioMetricsFactory
    {
        IPortfolioMetric Get(PortfolioMetricType type);
        IEnumerable<IPortfolioMetric> Get();
    }
}