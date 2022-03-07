using System.Collections.Generic;
using Service.Liquidity.Monitoring.Domain.Models.Checks;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Models.Hedging.Common
{
    public interface IHedgeStrategy
    {
        public HedgeStrategyType Type { get; set; }

        public HedgeCommandParams GetCommandParams(Portfolio portfolio, IEnumerable<PortfolioCheck> checks,
            HedgeStrategyParams strategyParams);
    }
}