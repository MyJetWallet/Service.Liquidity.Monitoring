using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets.Actions
{
    [DataContract]
    public class MakeHedgeMonitoringAction : MonitoringAction
    {
        [DataMember(Order = 1)] public override string TypeName { get; set; } = nameof(MakeHedgeMonitoringAction);
        [DataMember(Order = 2)] public override Dictionary<string, string> ParamValuesByName { get; set; }

        [DataMember(Order = 3)]
        public override ICollection<MonitoringActionParamInfo> ParamInfos { get; set; } =
            new List<MonitoringActionParamInfo>
            {
                new MonitoringActionParamInfo("HedgeStrategyType", MonitoringActionParamType.Int),
                new MonitoringActionParamInfo("HedgePercent", MonitoringActionParamType.Decimal),
            };
    }
}