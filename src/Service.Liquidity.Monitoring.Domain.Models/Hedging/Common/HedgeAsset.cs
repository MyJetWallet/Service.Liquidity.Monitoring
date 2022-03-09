﻿using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.Hedging.Common
{
    [DataContract]
    public class HedgeAsset
    {
        [DataMember(Order = 1)] public decimal Weight { get; set; }
        [DataMember(Order = 2)] public string Symbol { get; set; }
        [DataMember(Order = 3)] public decimal NetBalance { get; set; }
    }
}