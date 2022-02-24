using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Grpc.Models.RuleSets;

namespace Service.Liquidity.Monitoring.Grpc
{
    [ServiceContract]
    public interface IMonitoringRuleSetsManager
    {
        [OperationContract]
        Task<GetMonitoringRuleSetListResponse> GetListAsync(GetMonitoringRuleSetListRequest request);

        [OperationContract]
        Task<AddOrUpdateMonitoringRuleSetResponse> AddOrUpdateAsync(AddOrUpdateMonitoringRuleSetRequest request);

        [OperationContract]
        Task<GetMonitoringRuleSetResponse> GetAsync(GetMonitoringRuleSetRequest request);

        [OperationContract]
        Task<DeleteMonitoringRuleSetResponse> DeleteAsync(DeleteMonitoringRuleSetRequest request);
    }
}