using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;
using Service.Liquidity.Monitoring.Grpc;
using Service.Liquidity.Monitoring.Grpc.Models.Checks;

namespace Service.Liquidity.Monitoring.Services
{
    public class PortfolioChecksManager : IPortfolioChecksManager
    {
        private readonly ILogger<PortfolioChecksManager> _logger;
        private readonly IPortfolioChecksStorage _portfolioChecksStorage;
        private readonly IMonitoringRuleSetsStorage _monitoringRuleSetsStorage;

        public PortfolioChecksManager(
            ILogger<PortfolioChecksManager> logger,
            IPortfolioChecksStorage portfolioChecksStorage,
            IMonitoringRuleSetsStorage monitoringRuleSetsStorage
        )
        {
            _logger = logger;
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
                _logger.LogError(ex, "Failed GetList {@request}", request);
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
                _logger.LogError(ex, "Failed AddOrUpdate {@request}", request);

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
                _logger.LogError(ex, "Failed Get {@request}", request);

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

                return new DeletePortfolioCheckResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed Delete {@request}", request);

                return new DeletePortfolioCheckResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}