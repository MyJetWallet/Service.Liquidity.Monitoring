using System;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models
{
    [DataContract]
    public class AssetPortfolioStatusMessage
    {
        public const string TopicName = "jetwallet-liquidity-tradingportfolio-alarmstatus";
        
        [DataMember(Order = 1)] public string Asset { get; set; }
        [DataMember(Order = 2)] public DateTime ThresholdDate { get; set; }
        [DataMember(Order = 3)] public decimal CurrentValue { get; set; }
        [DataMember(Order = 4)] public decimal ThresholdValue { get; set; }
        [DataMember(Order = 5)] public bool IsAlarm { get; set; }
        [DataMember(Order = 6)] public string Message { get; set; }
    }
}