using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.Actions
{
    public interface IMonitoringAction
    {
        public string TypeName { get; set; }
        public Dictionary<string, string> ParamValuesByName { get; set; }
        public ICollection<MonitoringActionParamInfo> ParamInfos { get; set; }
    }
}