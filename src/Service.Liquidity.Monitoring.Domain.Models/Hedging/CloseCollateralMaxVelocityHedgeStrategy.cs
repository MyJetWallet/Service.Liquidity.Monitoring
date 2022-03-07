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
        public HedgeStrategyType Type { get; set; }

        public HedgeCommandParams GetCommandParams(Portfolio portfolio, IEnumerable<PortfolioCheck> checks,
            HedgeStrategyParams strategyParams)
        {
            var collaterals = portfolio.Assets
                .Where(a => a.Value.GetPositiveNetInUsd() != 0)
                .OrderBy(a => a.Value.NetBalanceInUsd)
                .ToArray();

            if (!collaterals.Any())
            {
                throw new Exception("Collaterals not found");
            }

            var positions = portfolio.Assets
                .Where(a => a.Value.GetNegativeNetInUsd() != 0)
                .OrderBy(a => a.Value.NetBalanceInUsd)
                .ToArray();

            if (!positions.Any())
            {
                throw new Exception("Collaterals not found");
            }

            var selectedAssets = checks.SelectMany(ch => ch.AssetSymbols).ToHashSet();
            var amount = positions
                .Where(p => selectedAssets.Contains(p.Key))
                .Sum(p => p.Value.NetBalance);

            return new HedgeCommandParams
            {
                Amount = amount * (strategyParams.AmountPercent / 100),
                SellAssetSymbol = collaterals.FirstOrDefault().Key,
                BuyAssetSymbol = positions.FirstOrDefault().Key
            };
        }
    }
}