using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Grpc.Models.Checks;

namespace Service.Liquidity.Monitoring.Grpc
{
    [ServiceContract]
    public interface IPortfolioChecksManager
    {
        [OperationContract]
        Task<GetPortfolioCheckListResponse> GetListAsync(GetPortfolioCheckListRequest request);

        [OperationContract]
        Task<AddOrUpdatePortfolioCheckResponse> AddOrUpdateAsync(AddOrUpdatePortfolioCheckRequest request);

        [OperationContract]
        Task<GetPortfolioCheckResponse> GetAsync(GetPortfolioCheckRequest request);

        [OperationContract]
        Task<DeletePortfolioCheckResponse> DeleteAsync(DeletePortfolioCheckRequest request);
    }
}