using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.Monitoring.Grpc.Models;

namespace Service.Liquidity.Monitoring.Grpc
{
    [ServiceContract]
    public interface IAssetPortfolioSettingsManager
    {
        [OperationContract]
        Task<GetAssetPortfolioSettingsResponse> GetAssetPortfolioSettingsAsync();
        
        [OperationContract]
        Task UpdateAssetPortfolioSettingsAsync(AssetPortfolioSettings settings);
    }
}
