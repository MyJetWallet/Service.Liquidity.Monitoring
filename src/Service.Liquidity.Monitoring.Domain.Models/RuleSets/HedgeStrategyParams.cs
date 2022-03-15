using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets
{
    [DataContract]
    public class HedgeStrategyParams
    {
        [DataMember(Order = 1)] public decimal AmountPercent { get; set; }
    }
}