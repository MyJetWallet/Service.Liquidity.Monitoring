using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models.Checks;

namespace Service.Liquidity.Monitoring.Grpc.Models.Checks
{
    [DataContract]
    public class GetPortfolioCheckListResponse
    {
        [DataMember(Order = 1)] public IEnumerable<PortfolioCheck> Items { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public bool IsError { get; set; }
    }
}