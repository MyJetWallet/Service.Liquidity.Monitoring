using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Mapster;
using Service.Liquidity.Monitoring.Domain.Models.Metrics.Common;
using Service.Liquidity.Monitoring.Domain.Models.Operators;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Models.Checks
{
    [DataContract]
    public class PortfolioCheck
    {
        [DataMember(Order = 1)] public string Id { get; set; }
        [DataMember(Order = 2)] public string Name { get; set; }
        [DataMember(Order = 3)] public IEnumerable<string> AssetSymbols { get; set; }
        [DataMember(Order = 4)] public IEnumerable<string> CompareAssetSymbols { get; set; }
        [DataMember(Order = 5)] public decimal TargetValue { get; set; }
        [DataMember(Order = 6)] public PortfolioMetricType MetricType { get; set; }
        [DataMember(Order = 7)] public CompareOperatorType OperatorType { get; set; }
        [DataMember(Order = 8)] public PortfolioCheckState CurrentState { get; set; }
        [DataMember(Order = 9)] public PortfolioCheckState PrevState { get; set; }
        [DataMember(Order = 10)] public string Description { get; set; }

        public void RefreshState(Portfolio portfolio, IPortfolioMetric metric)
        {
            if (metric == null || metric.Type != MetricType)
            {
                throw new Exception($"Provided invalid metric {metric?.Type} for check {Name}");
            }

            var metricParams = new PortfolioMetricParams
            {
                AssetSymbols = AssetSymbols ?? ArraySegment<string>.Empty,
                CompareAssetSymbols = CompareAssetSymbols ?? ArraySegment<string>.Empty
            };

            var metricValue = decimal.Round(metric.Calculate(portfolio, metricParams), 2);
            var compareOperator = new CompareOperator(OperatorType);
            var isActive = compareOperator.Compare(metricValue, TargetValue);

            if (PrevState?.IsActive != isActive)
            {
                PrevState = CurrentState.Adapt<PortfolioCheckState>();
            }

            if (CurrentState == null)
            {
                CurrentState = PortfolioCheckState.Create(isActive, metricValue);
            }
            else
            {
                CurrentState.Refresh(isActive, metricValue);
            }
        }
    }
}