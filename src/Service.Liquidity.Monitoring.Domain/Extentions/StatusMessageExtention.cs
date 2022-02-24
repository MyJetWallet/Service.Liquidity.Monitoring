using Service.Liquidity.Monitoring.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Extentions
{
    public static  class StatusMessageExtention
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