using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Services;
using Service.Liquidity.Monitoring.Grpc;
using Service.Liquidity.Monitoring.Grpc.Models.Checks;

namespace Service.Liquidity.Monitoring.Services
{
    public class PortfolioChecksManager : IPortfolioChecksManager
    {
        private readonly IPortfolioChecksStorage _portfolioChecksStorage;
        private readonly IMonitoringRuleSetsStorage _monitoringRuleSetsStorage;

        public PortfolioChecksManager(
            IPortfolioChecksStorage portfolioChecksStorage,
            IMonitoringRuleSetsStorage monitoringRuleSetsStorage
        )
        {
            _portfolioChecksStorage = portfolioChecksStorage;
            _monitoringRuleSetsStorage = monitoringRuleSetsStorage;
        }

        public async Task<GetPortfolioCheckListResponse> GetListAsync(GetPortfolioCheckListRequest request)
        {
            try
            {
                var items = await _portfolioChecksStorage.GetAsync();

                return new GetPortfolioCheckListResponse
                {
                    Items = items
                };
            }
            catch (Exception ex)
            {
                return new GetPortfolioCheckListResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<AddOrUpdatePortfolioCheckResponse> AddOrUpdateAsync(AddOrUpdatePortfolioCheckRequest request)
        {
            try
            {
                await _portfolioChecksStorage.AddOrUpdateAsync(request.Item);

                return new AddOrUpdatePortfolioCheckResponse();
            }
            catch (Exception ex)
            {
                return new AddOrUpdatePortfolioCheckResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<GetPortfolioCheckResponse> GetAsync(GetPortfolioCheckRequest request)
        {
            try
            {
                var item = await _portfolioChecksStorage.GetAsync(request.Id);

                return new GetPortfolioCheckResponse
                {
                    Item = item
                };
            }
            catch (Exception ex)
            {
                return new GetPortfolioCheckResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<DeletePortfolioCheckResponse> DeleteAsync(DeletePortfolioCheckRequest request)
        {
            try
            {
                await _portfolioChecksStorage.DeleteAsync(request.Id);
                var ruleSets = await _monitoringRuleSetsStorage.GetAsync();

                foreach (var ruleSet in ruleSets)
                {
                    var ruleSetChanged = false;

                    foreach (var rule in ruleSet.Rules)
                    {
                        if (rule.CheckIds.Contains(request.Id))
                        {
                            rule.CheckIds = new List<string>(rule.CheckIds.Where(id => id != request.Id));
                            ruleSetChanged = true;
                        }
                    }

                    if (ruleSetChanged)
                    {
                        await _monitoringRuleSetsStorage.AddOrUpdateAsync(ruleSet);
                    }
                }

                return new DeletePortfolioCheckResponse();
            }
            catch (Exception ex)
            {
                return new DeletePortfolioCheckResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}