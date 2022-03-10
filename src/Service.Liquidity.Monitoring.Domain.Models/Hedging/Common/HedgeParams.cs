﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.Hedging.Common
{
    [DataContract]
    public class HedgeParams
    {
        [DataMember(Order = 1)] public string BuyAssetSymbol { get; set; }
        [DataMember(Order = 2)] public List<HedgeAsset> SellAssets { get; set; } = new List<HedgeAsset>();
        [DataMember(Order = 3)] public decimal BuyVolume { get; set; }

        public bool Validate(out ICollection<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(BuyAssetSymbol))
            {
                errors.Add($"{nameof(BuyAssetSymbol)} are empty");
            }

            if (!SellAssets.Any())
            {
                errors.Add($"{nameof(SellAssets)} are empty");
            }

            if (BuyVolume <= 0)
            {
                errors.Add($"{nameof(BuyVolume)} must be bigger than 0");
            }

            return !errors.Any();
        }
    }
}