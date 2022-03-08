using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets
{
    [DataContract]
    public class MonitoringNotificationMessage
    {
        public const string SbTopicName = "jetwallet-liquidity-monitoring-notification-message";

        [DataMember(Order = 1)] public string ChannelId { get; set; }
        [DataMember(Order = 2)] public string Text { get; set; }
    }
}