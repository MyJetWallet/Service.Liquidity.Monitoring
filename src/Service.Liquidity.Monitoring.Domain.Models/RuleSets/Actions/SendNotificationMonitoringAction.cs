using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets.Actions
{
    [DataContract]
    public class SendNotificationMonitoringAction : MonitoringAction
    {
        [DataMember(Order = 1)]
        public override string TypeName { get; set; } = nameof(SendNotificationMonitoringAction);

        [DataMember(Order = 2)] public override Dictionary<string, string> ParamValuesByName { get; set; }

        [DataMember(Order = 3)]
        public override ICollection<MonitoringActionParamInfo> ParamInfos { get; set; } =
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