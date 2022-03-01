using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Services;
using Service.Liquidity.Monitoring.Grpc;
using Service.Liquidity.Monitoring.Grpc.Models.Checks;

namespace Service.Liquidity.Monitoring.Services
{
    public class PortfolioChecksManager : IPortfolioChecksManager
    {
        private readonly IPortfolioChecksStorage _portfolioChecksStorage;

        public PortfolioChecksManager(
            IPortfolioChecksStorage portfolioChecksStorage
        )
        {
            _portfolioChecksStorage = portfolioChecksStorage;
        }

        public async Task<GetPortfolioCheckListResponse> GetListAsync(GetPortfolioCheckListRequest request)
        {
            var items = await _portfolioChecksStorage.GetAsync();

            return new GetPortfolioCheckListResponse
            {
                Items = items
            };
        }

        public async Task<AddOrUpdatePortfolioCheckResponse> AddOrUpdateAsync(AddOrUpdatePortfolioCheckRequest request)
        {
            await _portfolioChecksStorage.AddOrUpdateAsync(request.Item);

            return new AddOrUpdatePortfolioCheckResponse();
        }

        public async Task<GetPortfolioCheckResponse> GetAsync(GetPortfolioCheckRequest request)
        {
            var item = await _portfolioChecksStorage.GetAsync(request.Id);

            return new GetPortfolioCheckResponse
            {
                Item = item
            };
        }

        public async Task<DeletePortfolioCheckResponse> DeleteAsync(DeletePortfolioCheckRequest request)
        {
            await _portfolioChecksStorage.DeleteAsync(request.Id);

            return new DeletePortfolioCheckResponse();
        }
    }
}