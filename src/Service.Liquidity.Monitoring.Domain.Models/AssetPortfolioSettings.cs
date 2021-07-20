using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models
{
    [DataContract]
    public class AssetPortfolioSettings
    {
        [DataMember(Order = 1)] public string Asset { get; set; }
        [DataMember(Order = 2)] public decimal NetWarningLevel { get; set; }
        [DataMember(Order = 3)] public decimal NetDangerLevel { get; set; }
        [DataMember(Order = 4)] public decimal NetCriticalLevel { get; set; }
    }
}
