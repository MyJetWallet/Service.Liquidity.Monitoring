using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models
{
    [DataContract]
    public class AssetPortfolioSettings
    {
        [DataMember(Order = 1)] public string Asset { get; set; }
        [DataMember(Order = 2)] public List<decimal> PositiveUpl { get; set; }
        [DataMember(Order = 3)] public List<decimal> NegativeUpl { get; set; }
        [DataMember(Order = 4)] public List<decimal> PositiveNetUsd { get; set; }
        [DataMember(Order = 5)] public List<decimal> NegativeNetUsd { get; set; }
    }
}
