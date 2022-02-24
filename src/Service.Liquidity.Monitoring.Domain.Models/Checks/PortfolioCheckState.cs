using System;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.Checks
{
    [DataContract]
    public class PortfolioCheckState
    {
        [DataMember(Order = 1)] public DateTime Date { get; set; }
        [DataMember(Order = 2)] public bool IsActive { get; set; }

        public PortfolioCheckState(bool isActive)
        {
            Date = DateTime.UtcNow;
            IsActive = isActive;
        }
    }
}