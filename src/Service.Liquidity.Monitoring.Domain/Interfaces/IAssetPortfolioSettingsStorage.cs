using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Interfaces
{
    public interface IAssetPortfolioSettingsStorage
    {
        Task<AssetPortfolioSettings> GetAssetPortfolioSettingsByAsset(string asset);
        List<AssetPortfolioSettings> GetAssetPortfolioSettings();
        Task UpdateAssetPortfolioSettingsAsync(AssetPortfolioSettings settings);
    }
}
