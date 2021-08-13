using System;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models
{
    [DataContract]
    public class AssetPortfolioStatus
    {
        [DataMember(Order = 1)] public string Asset { get; set; }
        [DataMember(Order = 2)] public DateTime UpdateDate { get; set; }
        [DataMember(Order = 3)] public decimal UplStrike { get; set; }
        [DataMember(Order = 4)] public decimal NetUsdStrike { get; set; }
        [DataMember(Order = 5)] public decimal Upl { get; set; }
        [DataMember(Order = 6)] public decimal NetUsd { get; set; }
    }
}