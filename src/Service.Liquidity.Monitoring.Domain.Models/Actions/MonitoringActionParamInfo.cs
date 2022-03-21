using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.Actions
{
    [DataContract]
    public class MonitoringActionParamInfo
    {
        [DataMember(Order = 1)] public string Name { get; set; }
        [DataMember(Order = 2)] public MonitoringActionParamType Type { get; set; }

        public MonitoringActionParamInfo()
        {
        }

        public MonitoringActionParamInfo(string name, MonitoringActionParamType type)
        {
            Name = name;
            Type = type;
        }
    }
}