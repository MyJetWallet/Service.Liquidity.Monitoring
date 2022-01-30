using System;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models
{
    [DataContract]
    public class Status
    {
        [DataMember(Order = 1)] public DateTime ThresholdDate { get; set; }
        [DataMember(Order = 2)] public decimal CurrentValue { get; set; }
        [DataMember(Order = 3)] public decimal ThresholdValue { get; set; }
        [DataMember(Order = 4)] public bool IsAlarm { get; set; }
    }
    
    
    [DataContract]
    public class AssetPortfolioStatus
    {
        [DataMember(Order = 1)] public string Asset { get; set; }
        [DataMember(Order = 2)] public Status Velocity { get; set; }
        [DataMember(Order = 3)] public Status VelocityRisk { get; set; }
    }
}