﻿using System;
using System.Collections.Generic;
using System.Linq;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.Monitoring.Domain.Models.Hedging.Common;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Models.Hedging
{
    public class ClosePositionMaxVelocityHedgeStrategy : IHedgeStrategy
    {
        public HedgeStrategyType Type { get; set; } = HedgeStrategyType.ClosePositionMaxVelocity;

        public HedgeParams CalculateHedgeParams(Portfolio portfolio, IEnumerable<PortfolioCheck> checks,
            HedgeStrategyParams strategyParams)
        {
            var selectedAssets = checks.SelectMany(ch => ch.AssetSymbols).ToHashSet();
            var selectedPositionAssets = portfolio.Assets
                .Select(a => a.Value)
                .Where(a => a.GetNegativeNetInUsd() != 0 && selectedAssets.Contains(a.Symbol))
                .OrderBy(a => a.DailyVelocityRiskInUsd)
                .ToList();

            var hedgeParams = new HedgeParams
            {
                BuyAssetSymbol = selectedPositionAssets.FirstOrDefault()?.Symbol,
                SellAssets = portfolio.Assets
                    .Select(a => a.Value)
                    .Where(a => a.GetPositiveNetInUsd() != 0 && !selectedAssets.Contains(a.Symbol))
                    .OrderBy(a => a.DailyVelocityRiskInUsd)
                    .Select(a => new HedgeAsset
                    {
                        Weight = a.DailyVelocityRiskInUsd,
                        Symbol = a.Symbol,
                        NetBalance = a.NetBalanceInUsd
                    })
                    .ToList(),
                BuyVolume = Math.Abs(selectedPositionAssets.Sum(a => a.NetBalance)) *
                            (strategyParams.AmountPercent / 100)
            };

            return hedgeParams;
        }
    }
}