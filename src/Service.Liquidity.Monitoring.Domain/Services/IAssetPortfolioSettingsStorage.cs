using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Services
{
    public interface IAssetPortfolioSettingsStorage
    {
        AssetPortfolioSettings GetAssetPortfolioSettingsByAsset(string asset);
        List<AssetPortfolioSettings> GetAssetPortfolioSettings();
        Task UpdateAssetPortfolioSettingsAsync(AssetPortfolioSettings settings);
    }
}
