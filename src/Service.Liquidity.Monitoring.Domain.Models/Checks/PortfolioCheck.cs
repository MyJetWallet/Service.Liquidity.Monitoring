using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Checks.Operators;
using Service.Liquidity.Monitoring.Domain.Models.Checks.Strategies;
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
        [DataMember(Order = 6)] public PortfolioCheckStrategyType StrategyType { get; set; }
        [DataMember(Order = 7)] public CheckOperatorType OperatorType { get; set; }
        [DataMember(Order = 8)] public IPortfolioCheckStrategy Strategy { get; set; }
        [DataMember(Order = 9)] public PortfolioCheckState CurrentState { get; set; }
        [DataMember(Order = 10)] public PortfolioCheckState PrevState { get; set; }

        public PortfolioCheck(IPortfolioCheckStrategy strategy)
        {
            if (strategy.Type != StrategyType)
            {
                throw new Exception("Provided invalid strategy");
            }

            Strategy = strategy;
        }

        public bool Matches(Portfolio portfolio)
        {
            var strategyParams = new PortfolioCheckStrategyParams
            {
                AssetSymbols = AssetSymbols,
                TargetValue = TargetValue,
                CompareAssetSymbols = CompareAssetSymbols
            };

            var result = Strategy.Execute(portfolio, strategyParams, OperatorType);
            PrevState = CurrentState;
            CurrentState = new PortfolioCheckState(result);

            return result;
        }
    }
}