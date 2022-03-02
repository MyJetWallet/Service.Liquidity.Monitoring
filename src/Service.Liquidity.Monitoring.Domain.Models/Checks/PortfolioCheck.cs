using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Humanizer;
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

        public override string ToString()
        {
            const string inactiveSymbol = "\U0001F7E2"; // green circle
            const string activeSymbol = "\U0001F534"; // red circle

            var title = CurrentState.IsActive
                ? $"{activeSymbol} Check <b>{Name}</b> is active"
                : $"{inactiveSymbol} Check <b>{Name}</b> is inactive";

            return $"{title}{Environment.NewLine}" +
                   $"{MetricType.Humanize()} value: <b>{CurrentState.MetricValue}</b>{Environment.NewLine}" +
                   $"Target value: <b>{TargetValue}</b>";
        }

        public string GenerateDescription()
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                return ToString();
            }

            return $"Check {Name} is {(CurrentState.IsActive ? "active" : "inactive")}:{Environment.NewLine}{Description}";
        }

        public bool Execute(Portfolio portfolio, IPortfolioMetric metric)
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

            var metricValue = metric.Calculate(portfolio, metricParams);
            var compareOperator = new CompareOperator(OperatorType);
            var isActive = compareOperator.Compare(metricValue, TargetValue);
            var isChanged = false;
            var isActiveChangedDate = CurrentState?.IsActiveChangedDate;

            if (PrevState?.IsActive != isActive)
            {
                isActiveChangedDate = DateTime.UtcNow;
                isChanged = true;
                PrevState = CurrentState;
            }

            CurrentState = new PortfolioCheckState(isActive, isActiveChangedDate, metricValue);

            return isChanged;
        }
    }
}