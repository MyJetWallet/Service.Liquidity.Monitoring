using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Services;
using Service.Liquidity.Monitoring.Grpc;
using Service.Liquidity.Monitoring.Grpc.Models.RuleSets;

namespace Service.Liquidity.Monitoring.Services
{
    public class MonitoringRuleSetsManager : IMonitoringRuleSetsManager
    {
        private readonly IMonitoringRuleSetsStorage _monitoringRuleSetsStorage;
        private readonly IPortfolioChecksStorage _portfolioChecksStorage;

        public MonitoringRuleSetsManager(
            IMonitoringRuleSetsStorage monitoringRuleSetsStorage,
            IPortfolioChecksStorage portfolioChecksStorage
        )
        {
            _monitoringRuleSetsStorage = monitoringRuleSetsStorage;
            _portfolioChecksStorage = portfolioChecksStorage;
        }

        public async Task<GetMonitoringRuleSetListResponse> GetListAsync(GetMonitoringRuleSetListRequest request)
        {
            var items = await _monitoringRuleSetsStorage.GetAsync();

            return new GetMonitoringRuleSetListResponse
            {
                Items = items
            };
        }

        public async Task<AddOrUpdateMonitoringRuleSetResponse> AddOrUpdateAsync(
            AddOrUpdateMonitoringRuleSetRequest request)
        {
            await _monitoringRuleSetsStorage.AddOrUpdateAsync(request.Item);

            return new AddOrUpdateMonitoringRuleSetResponse();
        }

        public async Task<GetMonitoringRuleSetResponse> GetAsync(GetMonitoringRuleSetRequest request)
        {
            var item = await _monitoringRuleSetsStorage.GetAsync(request.Id);

            return new GetMonitoringRuleSetResponse
            {
                Item = item
            };
        }

        public async Task<DeleteMonitoringRuleSetResponse> DeleteAsync(DeleteMonitoringRuleSetRequest request)
        {
            await _monitoringRuleSetsStorage.DeleteAsync(request.Id);

            return new DeleteMonitoringRuleSetResponse();
        }
    }
}