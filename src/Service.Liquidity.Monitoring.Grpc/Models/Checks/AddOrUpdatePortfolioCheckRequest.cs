using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Checks;

namespace Service.Liquidity.Monitoring.Grpc.Models.Checks
{
    [DataContract]
    public class AddOrUpdatePortfolioCheckRequest
    {
        [DataMember(Order = 1)] public PortfolioCheck Item { get; set; }
    }
}