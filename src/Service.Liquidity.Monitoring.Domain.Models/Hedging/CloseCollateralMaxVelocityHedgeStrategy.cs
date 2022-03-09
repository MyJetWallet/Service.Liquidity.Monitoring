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
            var selectedAssets = checks.SelectMany(ch => ch.AssetSymbols).ToHashSet();

            var hedgeParams = new HedgeParams
            {
                BuyAssets = portfolio.Assets
                    .Select(a => a.Value)
                    .Where(a => a.GetNegativeNetInUsd() != 0 && selectedAssets.Contains(a.Symbol))
                    .OrderBy(a => a.DailyVelocityRiskInUsd)
                    .Select(a => new HedgeAsset
                    {
                        Weight = a.DailyVelocityRiskInUsd,
                        Symbol = a.Symbol,
                        NetBalanceInUsd = a.NetBalanceInUsd
                    })
                    .ToList(),
                SellAssets = portfolio.Assets
                    .Select(a => a.Value)
                    .Where(a => a.GetPositiveNetInUsd() != 0 && !selectedAssets.Contains(a.Symbol))
                    .OrderBy(a => a.DailyVelocityRiskInUsd)
                    .Select(a => new HedgeAsset
                    {
                        Weight = a.DailyVelocityRiskInUsd,
                        Symbol = a.Symbol,
                        NetBalanceInUsd = a.NetBalanceInUsd
                    })
                    .ToList(),
                AmountPercent = strategyParams.AmountPercent
            };

            return hedgeParams;
        }
    }
}