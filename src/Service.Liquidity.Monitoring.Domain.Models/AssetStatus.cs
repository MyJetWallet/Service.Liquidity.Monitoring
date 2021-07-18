using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models
{
    [DataContract]
    public enum AssetStatus
    {
        [DataMember(Order = 1)] Normal,
        [DataMember(Order = 2)] Warning,
        [DataMember(Order = 3)] Danger,
        [DataMember(Order = 4)] Critical
    }
}