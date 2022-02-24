using System.Linq;
using Service.Liquidity.Monitoring.Domain.Models.Checks.Operators;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Models.Checks.Strategies
{
    public class CollatAmountPortfolioCheckStrategy : IPortfolioCheckStrategy
    {
        public PortfolioCheckStrategyType Type { get; set; } = PortfolioCheckStrategyType.CollatAmount;

        public bool Execute(Portfolio portfolio, PortfolioCheckStrategyParams portfolioCheckParams, CheckOperatorType operatorType)
        {
            var sign = new CheckOperator(operatorType);
            
            var assetCollatAmount = portfolio.Assets
                .Where(a => portfolioCheckParams.AssetSymbols.Contains(a.Key))
                .Sum(x => x.Value.DailyVelocity);
            var compareAssetCollatAmount = portfolio.Assets
                .Where(a => portfolioCheckParams.CompareAssetSymbols.Contains(a.Key))
                .Sum(x => x.Value.DailyVelocity);
                    
            return sign.Compare(assetCollatAmount - compareAssetCollatAmount, portfolioCheckParams.TargetValue);
        }
    }
}