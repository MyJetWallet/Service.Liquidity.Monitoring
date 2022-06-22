using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Grpc.Models.Backups;
using Service.Liquidity.Monitoring.Grpc.Models.Rules;

namespace Service.Liquidity.Monitoring.Grpc
{
    [ServiceContract]
    public interface IMonitoringRulesBackupsManager
    {
        [OperationContract]
        Task<GetMonitoringRulesBackupInfosResponse> GetInfosAsync(GetMonitoringRulesBackupInfosRequest request);

        [OperationContract]
        Task<AddOrUpdateMonitoringRulesBackupResponse>
            AddOrUpdateAsync(AddOrUpdateMonitoringRulesBackupRequest request);

        [OperationContract]
        Task<GetMonitoringRulesBackupResponse> GetAsync(GetMonitoringRulesBackupRequest request);

        [OperationContract]
        Task<DeleteMonitoringRulesBackupResponse> DeleteAsync(DeleteMonitoringRulesBackupRequest request);

        [OperationContract]
        Task<ApplyMonitoringRulesBackupResponse> ApplyAsync(ApplyMonitoringRulesBackupRequest request);
    }
}