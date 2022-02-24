namespace Service.Liquidity.Monitoring.Domain.Models.Checks.Strategies
{
    public enum PortfolioCheckStrategyType
    {
        CollatAmount,
        CollatDollar,
        VelocityDollar,
        VelocityPercent,
        PnlDollar,
        PositionDollar,
        PositionAmount
    }
}