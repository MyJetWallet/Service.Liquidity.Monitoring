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
        [DataMember(Order = 4)] public DateTime? IsActiveChangedDate { get; set; }
        
        public static PortfolioCheckState Create(bool isActive, decimal metricValue)
        {
            return new PortfolioCheckState
            {
                Date = DateTime.UtcNow,
                IsActive = isActive,
                IsActiveChangedDate = DateTime.UtcNow,
                MetricValue = metricValue
            };
        }
        
        public void Refresh(bool isActive, decimal metricValue)
        {
            Date = DateTime.UtcNow;

            if (IsActive != isActive)
            {
                IsActiveChangedDate = Date;
            }

            IsActive = isActive;
            MetricValue = metricValue;
        }
    }
}