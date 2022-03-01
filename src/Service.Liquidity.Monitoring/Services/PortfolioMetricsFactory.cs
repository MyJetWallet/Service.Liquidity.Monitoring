using System;
using System.Collections.Generic;
using Service.Liquidity.Monitoring.Domain.Models.Metrics;
using Service.Liquidity.Monitoring.Domain.Models.Metrics.Common;
using Service.Liquidity.Monitoring.Domain.Services;

namespace Service.Liquidity.Monitoring.Services
{
    public class PortfolioMetricsFactory : IPortfolioMetricsFactory
    {
        private readonly Dictionary<PortfolioMetricType, IPortfolioMetric> _metrics;

        public PortfolioMetricsFactory()
        {
            _metrics = new Dictionary<PortfolioMetricType, IPortfolioMetric>
            {
                { PortfolioMetricType.CollateralPercent, new CollateralPercentPortfolioMetric() },
                { PortfolioMetricType.CollateralUsd, new CollateralUsdPortfolioMetric() },
                { PortfolioMetricType.PnlUsd, new PnlUsdPortfolioMetric() },
                { PortfolioMetricType.PositionPercent, new PositionPercentPortfolioMetric() },
                { PortfolioMetricType.PositionUsd, new PositionUsdPortfolioMetric() },
                { PortfolioMetricType.DailyVelocityPercent, new DailyVelocityPercentPortfolioMetric() },
                { PortfolioMetricType.DailyVelocityUsd, new DailyVelocityUsdPortfolioMetric() },
            };
        }

        public IEnumerable<IPortfolioMetric> Get()
        {
            return _metrics.Values;
        }

        public IPortfolioMetric Get(PortfolioMetricType type)
        {
            if (_metrics.TryGetValue(type, out var metric))
            {
                return metric;
            }

            throw new Exception($"Metric {type.ToString()} Not Found");
        }
    }
}