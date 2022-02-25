using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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

        public bool Matches(Portfolio portfolio, Dictionary<PortfolioMetricType, IPortfolioMetric> metrics)
        {
            if (!metrics.TryGetValue(MetricType, out var metric))
            {
                throw new Exception($"Metric {MetricType.ToString()} not found for check {Name}");
            }
            
            var metricParams = new PortfolioMetricParams
            {
                AssetSymbols = AssetSymbols,
                CompareAssetSymbols = CompareAssetSymbols
            };

            var metricResult = metric.Calculate(portfolio, metricParams);
            var compareOperator = new CompareOperator(OperatorType);
            var result = compareOperator.Compare(metricResult, TargetValue);
            PrevState = CurrentState;
            CurrentState = new PortfolioCheckState(result);

            return result;
        }
    }
}