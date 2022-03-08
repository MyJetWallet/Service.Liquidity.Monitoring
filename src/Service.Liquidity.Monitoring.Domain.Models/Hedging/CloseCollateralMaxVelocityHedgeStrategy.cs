using System;
using System.Collections.Generic;
using System.Linq;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.Hedging.Common;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Models.Hedging
{
    public class CloseCollateralMaxVelocityHedgeStrategy : IHedgeStrategy
    {
        public HedgeStrategyType Type { get; set; } = HedgeStrategyType.CloseCollateralMaxVelocity;

        public HedgeParams CalculateHedgeParams(Portfolio portfolio, IEnumerable<PortfolioCheck> checks,
            HedgeStrategyParams strategyParams)
        {
            var collaterals = portfolio.Assets
                .Select(a => a.Value)
                .Where(a => a.GetPositiveNetInUsd() != 0)
                .OrderBy(a => a.DailyVelocityRiskInUsd)
                .ToArray();

            var positions = portfolio.Assets
                .Select(a => a.Value)
                .Where(a => a.GetNegativeNetInUsd() != 0)
                .OrderBy(a => a.DailyVelocityRiskInUsd)
                .ToArray();

            var selectedAssets = checks.SelectMany(ch => ch.AssetSymbols).ToHashSet();
            var targetPositions = positions.Where(asset => selectedAssets.Contains(asset.Symbol)).ToArray();

            var hedgeParams = new HedgeParams();

            for (var i = 0; i < targetPositions.Length; i++)
            {
                hedgeParams.BuyAssets.Add(new HedgeParams.BuyAsset
                {
                    Priority = i,
                    Symbol = positions[i].Symbol,
                    NetBalanceInUsd = positions[i].NetBalanceInUsd
                });
            }

            for (var i = 0; i < collaterals.Length; i++)
            {
                hedgeParams.SellAssets.Add(new HedgeParams.SellAsset
                {
                    Priority = i,
                    Symbol = collaterals[i].Symbol,
                    SellAmountInUsd = collaterals[i].NetBalanceInUsd * (strategyParams.AmountPercent / 100)
                });
            }

            return hedgeParams;
        }
    }
}