using Service.Liquidity.Monitoring.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Extensions
{
    public static class StatusMessageExtension
    {
        public static AssetPortfolioStatusMessage PrepareMessage(
            this Status actualAssetStatus,
            string asset,
            string message)
        {
            return new AssetPortfolioStatusMessage
            {
                Asset = asset,
                ThresholdDate = actualAssetStatus.ThresholdDate,
                CurrentValue = actualAssetStatus.CurrentValue,
                ThresholdValue = actualAssetStatus.ThresholdValue,
                IsAlarm = actualAssetStatus.IsAlarm,
                Message = message
            };
        }
    }
}