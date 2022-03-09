using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.Hedging.Common
{
    [DataContract]
    public class HedgeParams
    {
        [DataMember(Order = 1)] public List<HedgeAsset> BuyAssets { get; set; } = new List<HedgeAsset>();
        [DataMember(Order = 2)] public List<HedgeAsset> SellAssets { get; set; } = new List<HedgeAsset>();
        [DataMember(Order = 3)] public decimal AmountPercent { get; set; }

        public decimal GetAmountInUsd()
        {
            return Math.Abs(BuyAssets.Sum(a => a.NetBalanceInUsd)) * (AmountPercent / 100);
        }

        public bool Validate(out ICollection<string> errors)
        {
            errors = new List<string>();

            if (!BuyAssets.Any())
            {
                errors.Add($"{nameof(BuyAssets)} are empty");
            }

            if (!SellAssets.Any())
            {
                errors.Add($"{nameof(SellAssets)} are empty");
            }

            if (AmountPercent <= 0)
            {
                errors.Add($"{nameof(AmountPercent)} must be bigger than 0");
            }

            return !errors.Any();
        }
    }
}