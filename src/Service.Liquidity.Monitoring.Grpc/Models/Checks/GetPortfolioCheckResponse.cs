using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Checks;

namespace Service.Liquidity.Monitoring.Grpc.Models.Checks
{
    [DataContract]
    public class GetPortfolioCheckResponse
    {
        [DataMember(Order = 1)] public PortfolioCheck Item { get; set; }
    }
}