using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Grpc.Models;

namespace Service.Liquidity.Monitoring.Grpc
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract]
        Task<HelloMessage> SayHelloAsync(HelloRequest request);
    }
}