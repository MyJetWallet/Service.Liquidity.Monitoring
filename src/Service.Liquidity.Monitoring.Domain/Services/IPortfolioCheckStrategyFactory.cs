using Service.Liquidity.Monitoring.Domain.Models.Checks.Strategies;

namespace Service.Liquidity.Monitoring.Domain.Services
{
    public interface IPortfolioCheckStrategyFactory
    {
        IPortfolioCheckStrategy Get(PortfolioCheckStrategyType type);
    }
}