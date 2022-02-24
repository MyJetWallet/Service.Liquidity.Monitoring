using Service.Liquidity.Monitoring.Domain.Models.Checks.Operators;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Models.Checks.Strategies
{
    public interface IPortfolioCheckStrategy
    {
        public PortfolioCheckStrategyType Type { get; set; }

        public bool Execute(Portfolio portfolio, PortfolioCheckStrategyParams portfolioCheckParams, CheckOperatorType operatorType);
    }
}