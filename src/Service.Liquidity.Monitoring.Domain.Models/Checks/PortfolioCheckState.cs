using System;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.Checks
{
    [DataContract]
    public class PortfolioCheckState
    {
        [DataMember(Order = 1)] public DateTime Date { get; set; }
        [DataMember(Order = 2)] public bool IsActive { get; set; }
        [DataMember(Order = 3)] public decimal MetricValue { get; set; }
        
        public PortfolioCheckState() {}

        public PortfolioCheckState(bool isActive, decimal metricValue)
        {
            Date = DateTime.UtcNow;
            IsActive = isActive;
            MetricValue = metricValue;
        }
    }
}