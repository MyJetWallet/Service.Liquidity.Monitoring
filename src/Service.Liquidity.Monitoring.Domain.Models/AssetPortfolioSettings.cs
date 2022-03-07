using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models
{
    [DataContract]
    public class AssetPortfolioSettings
    {
        [DataMember(Order = 1)] public string Asset { get; set; }
        [DataMember(Order = 2)] public decimal VelocityMin { get; set; }
        [DataMember(Order = 3)] public decimal VelocityMax { get; set; }
        [DataMember(Order = 4)] public decimal VelocityRiskUsdMin { get; set; }
        [DataMember(Order = 5)] public decimal TotalNegativeBalanceInUsdMin { get; set; }
        [DataMember(Order = 6)] public decimal TotalNegativeBalanceInPercent { get; set; }
    }
}
