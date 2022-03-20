using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets.Actions
{
    [DataContract]
    public class SendNotificationMonitoringAction : IMonitoringAction
    {
        [DataMember(Order = 1)] public string TypeName { get; set; } = nameof(SendNotificationMonitoringAction);

        [DataMember(Order = 2)]
        public Dictionary<string, string> ParamValuesByName { get; set; } = new Dictionary<string, string>();

        [DataMember(Order = 3)]
        public ICollection<MonitoringActionParamInfo> ParamInfos { get; set; } =
            new List<MonitoringActionParamInfo>
            {
                new MonitoringActionParamInfo(nameof(NotificationChannelId), MonitoringActionParamType.String),
            };

        [DataMember(Order = 4)]
        public string NotificationChannelId
        {
            get => ParamValuesByName[nameof(NotificationChannelId)];
            set => ParamValuesByName[nameof(NotificationChannelId)] = value;
        }
    }
}