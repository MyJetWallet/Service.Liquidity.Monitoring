using System;
using Service.Liquidity.Monitoring.Domain.Models.Checks.Strategies;
using Service.Liquidity.Monitoring.Domain.Services;

namespace Service.Liquidity.Monitoring.Services
{
    public  class PortfolioCheckStrategyFactory : IPortfolioCheckStrategyFactory
    {
        public IPortfolioCheckStrategy Get(PortfolioCheckStrategyType type)
        {
            switch (type)
            {
                case PortfolioCheckStrategyType.CollatAmount: return new CollatAmountPortfolioCheckStrategy();
                default: throw new NotSupportedException($"{type.ToString()}");
            }
        }
    }
}