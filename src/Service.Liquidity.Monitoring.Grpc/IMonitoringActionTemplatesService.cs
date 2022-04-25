using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Grpc.Models.ActionTemplates;

namespace Service.Liquidity.Monitoring.Grpc;

[ServiceContract]
public interface IMonitoringActionTemplatesService
{
    [OperationContract]
    Task<GetMonitoringActionTemplateListResponse> GetListAsync(GetMonitoringActionTemplateListRequest request);

    [OperationContract]
    Task<GetMonitoringActionTemplateResponse> GetAsync(GetMonitoringActionTemplateRequest request);
}