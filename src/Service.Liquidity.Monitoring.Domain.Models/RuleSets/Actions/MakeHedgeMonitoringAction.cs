using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets.Actions
{
    [DataContract]
    public class MakeHedgeMonitoringAction : IMonitoringAction
    {
        [DataMember(Order = 1)] public string TypeName { get; set; } = nameof(MakeHedgeMonitoringAction);

        [DataMember(Order = 2)]
        public Dictionary<string, string> ParamValuesByName { get; set; } = new Dictionary<string, string>();

        [DataMember(Order = 3)]
        public ICollection<MonitoringActionParamInfo> ParamInfos { get; set; } =
            new List<MonitoringActionParamInfo>
            {
                new MonitoringActionParamInfo("HedgeStrategyType", MonitoringActionParamType.Int),
                new MonitoringActionParamInfo("HedgePercent", MonitoringActionParamType.Decimal),
            };
    }
}