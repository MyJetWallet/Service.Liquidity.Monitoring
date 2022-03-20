namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets.Actions
{
    public class MonitoringActionParamInfo
    {
        public string Name { get; set; }
        public MonitoringActionParamType Type { get; set; }

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