using System;
using Service.Liquidity.Monitoring.Domain.Models.Metrics;
using Service.Liquidity.Monitoring.Domain.Models.Metrics.Common;
using Service.Liquidity.Monitoring.Domain.Services;

namespace Service.Liquidity.Monitoring.Services
{
    public  class PortfolioMetricFactory : IPortfolioMetricFactory
    {
        public IPortfolioMetric Get(PortfolioMetricType type)
        {
            switch (type)
            {
                case PortfolioMetricType.CollatAmount: return new CollatAmountPortfolioMetric();
                default: throw new NotSupportedException($"{type.ToString()}");
            }
        }
    }
}