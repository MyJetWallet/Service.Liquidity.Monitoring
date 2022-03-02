using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Grpc.Models.Checks
{
    [DataContract]
    public class DeletePortfolioCheckResponse
    {
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 1)] public bool IsError { get; set; }
    }
}