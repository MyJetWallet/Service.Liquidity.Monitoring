using System;
using System.Linq;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Grpc;
using Service.Liquidity.Monitoring.Grpc.Models.Rules;
using Service.Liquidity.Monitoring.Grpc.Models.RuleSets;
using Service.Liquidity.TradingPortfolio.Grpc;

namespace Service.Liquidity.Monitoring.Services
{
    public class MonitoringRulesManager : IMonitoringRulesManager
    {
        private readonly IMonitoringRulesStorage _monitoringRulesStorage;

        public MonitoringRulesManager(
            IMonitoringRulesStorage monitoringRulesStorage
        )
        {
            _monitoringRulesStorage = monitoringRulesStorage;
        }

        public async Task<GetMonitoringRuleListResponse> GetListAsync(GetMonitoringRuleListRequest request)
        {
            try
            {
                var items = (await _monitoringRulesStorage.GetAsync())?.ToArray();

                return new GetMonitoringRuleListResponse
                {
                    Items = items
                };
            }
            catch (Exception ex)
            {
                return new GetMonitoringRuleListResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<AddOrUpdateMonitoringRuleResponse> AddOrUpdateAsync(
            AddOrUpdateMonitoringRuleRequest request)
        {
            try
            {
                await _monitoringRulesStorage.AddOrUpdateAsync(request.Item);

                return new AddOrUpdateMonitoringRuleResponse();
            }
            catch (Exception ex)
            {
                return new AddOrUpdateMonitoringRuleResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<GetMonitoringRuleResponse> GetAsync(GetMonitoringRuleRequest request)
        {
            try
            {
                var item = await _monitoringRulesStorage.GetAsync(request.MonitoringRuleId);

                return new GetMonitoringRuleResponse
                {
                    Item = item
                };
            }
            catch (Exception ex)
            {
                return new GetMonitoringRuleResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<DeleteMonitoringRuleResponse> DeleteAsync(DeleteMonitoringRuleRequest request)
        {
            try
            {
                await _monitoringRulesStorage.DeleteAsync(request.MonitoringRuleId);

                return new DeleteMonitoringRuleResponse();
            }
            catch (Exception ex)
            {
                return new DeleteMonitoringRuleResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}