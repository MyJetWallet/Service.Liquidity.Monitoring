using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models;

namespace Service.Liquidity.Monitoring.Grpc.Models
{
    [DataContract]
    public class HelloMessage : IHelloMessage
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }
}