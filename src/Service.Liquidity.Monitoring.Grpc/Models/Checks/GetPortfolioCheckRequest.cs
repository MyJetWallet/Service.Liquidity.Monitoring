using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Grpc.Models.Checks
{
    [DataContract]
    public class GetPortfolioCheckRequest
    {
        [DataMember(Order = 1)] public string Id { get; set; }
    }
}