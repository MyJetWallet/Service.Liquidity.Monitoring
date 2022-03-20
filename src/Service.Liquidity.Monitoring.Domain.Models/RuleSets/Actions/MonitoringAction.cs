using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets.Actions
{
    [DataContract]
    public abstract class MonitoringAction
    {
        [DataMember(Order = 1)] public virtual string TypeName { get; set; }
        [DataMember(Order = 2)] public virtual Dictionary<string, string> ParamValuesByName { get; set; }
        [DataMember(Order = 3)] public virtual ICollection<MonitoringActionParamInfo> ParamInfos { get; set; }
    }
}