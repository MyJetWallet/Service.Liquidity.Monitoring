using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Grpc.Models.Rules;
using Service.Liquidity.Monitoring.Grpc.Models.RuleSets;

namespace Service.Liquidity.Monitoring.Grpc
{
    [ServiceContract]
    public interface IMonitoringRulesManager
    {
        [OperationContract]
        Task<GetMonitoringRuleListResponse> GetListAsync(GetMonitoringRuleListRequest request);

        [OperationContract]
        Task<AddOrUpdateMonitoringRuleResponse> AddOrUpdateAsync(AddOrUpdateMonitoringRuleRequest request);

        [OperationContract]
        Task<GetMonitoringRuleResponse> GetAsync(GetMonitoringRuleRequest request);

        [OperationContract]
        Task<DeleteMonitoringRuleResponse> DeleteAsync(DeleteMonitoringRuleRequest request);
    }
}