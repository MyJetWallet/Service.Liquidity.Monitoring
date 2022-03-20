using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets.Actions
{
    public interface IMonitoringAction
    {
        [DataMember(Order = 1)] public string TypeName { get; set; }
        [DataMember(Order = 2)] public Dictionary<string, string> ParamValuesByName { get; set; }
        [DataMember(Order = 3)] public ICollection<MonitoringActionParamInfo> ParamInfos { get; set; }

        public MonitoringAction ToBase();

    }
}