using System;
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
            try
            {
                var items = await _monitoringRuleSetsStorage.GetAsync();

                return new GetMonitoringRuleSetListResponse
                {
                    Items = items
                };
            }
            catch (Exception ex)
            {
                return new GetMonitoringRuleSetListResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<AddOrUpdateMonitoringRuleSetResponse> AddOrUpdateAsync(
            AddOrUpdateMonitoringRuleSetRequest request)
        {
            try
            {
                await _monitoringRuleSetsStorage.AddOrUpdateAsync(request.Item);

                return new AddOrUpdateMonitoringRuleSetResponse();
            }
            catch (Exception ex)
            {
                return new AddOrUpdateMonitoringRuleSetResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<GetMonitoringRuleSetResponse> GetAsync(GetMonitoringRuleSetRequest request)
        {
            try
            {
                var item = await _monitoringRuleSetsStorage.GetAsync(request.Id);

                return new GetMonitoringRuleSetResponse
                {
                    Item = item
                };
            }
            catch (Exception ex)
            {
                return new GetMonitoringRuleSetResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<DeleteMonitoringRuleSetResponse> DeleteAsync(DeleteMonitoringRuleSetRequest request)
        {
            try
            {
                await _monitoringRuleSetsStorage.DeleteAsync(request.Id);

                return new DeleteMonitoringRuleSetResponse();
            }
            catch (Exception ex)
            {
                return new DeleteMonitoringRuleSetResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}