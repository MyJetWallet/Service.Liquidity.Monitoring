using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Liquidity.Monitoring.Domain.Models;

namespace Service.Liquidity.Monitoring.Grpc.Models
{
    [DataContract]
    public class GetAssetPortfolioSettingsResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public List<AssetPortfolioSettings> Settings { get; set; }
    }
}
