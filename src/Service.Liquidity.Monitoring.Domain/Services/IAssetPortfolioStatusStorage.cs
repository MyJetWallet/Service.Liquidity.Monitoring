using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Services
{
    public interface IAssetPortfolioStatusStorage
    {
        AssetPortfolioStatus GetAssetPortfolioStatusByAsset(string asset);
        List<AssetPortfolioStatus> GetAssetPortfolioStatuses();
        Task UpdateAssetPortfolioStatusAsync(AssetPortfolioStatus assetPortfolioStatus);
    }
}
