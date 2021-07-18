using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.Monitoring.Domain.Services;
using Service.Liquidity.Monitoring.Grpc;
using Service.Liquidity.Monitoring.Grpc.Models;

namespace Service.Liquidity.Monitoring.Services
{
    public class AssetPortfolioSettingsManager : IAssetPortfolioSettingsManager
    {
        private readonly IAssetPortfolioSettingsStorage _assetPortfolioSettingsStorage;

        public AssetPortfolioSettingsManager(IAssetPortfolioSettingsStorage assetPortfolioSettingsStorage)
        {
            _assetPortfolioSettingsStorage = assetPortfolioSettingsStorage;
        }

        public async Task<GetAssetPortfolioSettingsResponse> GetAssetPortfolioSettingsAsync()
        {
            var settings = _assetPortfolioSettingsStorage.GetAssetPortfolioSettings();

            if (settings == null || settings.Count == 0)
                return new GetAssetPortfolioSettingsResponse()
                {
                    Success = false,
                    ErrorMessage = "Asset settings not found"
                };
            
            var response = new GetAssetPortfolioSettingsResponse()
            {
                Settings = settings,
                Success = true
            };
            return response;
        }

        public Task UpdateAssetPortfolioSettingsAsync(AssetPortfolioSettings settings)
        {
            return _assetPortfolioSettingsStorage.UpdateAssetPortfolioSettingsAsync(settings);
        }
    }
}
