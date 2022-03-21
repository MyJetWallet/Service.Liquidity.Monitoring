using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.Actions
{
    [DataContract]
    public class MonitoringAction : IMonitoringAction
    {
        [DataMember(Order = 1)] public string TypeName { get; set; }
        [DataMember(Order = 2)] public Dictionary<string, string> ParamValuesByName { get; set; }
        [DataMember(Order = 3)] public ICollection<MonitoringActionParamInfo> ParamInfos { get; set; }
        
        public MonitoringAction() {}

        public MonitoringAction(IMonitoringAction monitoringAction)
        {
            TypeName = monitoringAction.TypeName;
            ParamInfos = monitoringAction.ParamInfos;
            ParamValuesByName = monitoringAction.ParamValuesByName;
        }

        public T MapTo<T>() where T: IMonitoringAction
        {
            var instance = (T)Activator.CreateInstance(typeof(T));

            if (instance == null)
            {
                throw new Exception($"Failed to map MonitoringAction to {nameof(T)}");
            }
            
            instance.ParamInfos = ParamInfos;
            instance.TypeName = TypeName;
            instance.ParamValuesByName = ParamValuesByName;

            return instance;
        }
    }
}