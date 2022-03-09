using System;
using System.Collections.Generic;
using System.Linq;

namespace Service.Liquidity.Monitoring.Domain.Models.Hedging.Common
{
    public class HedgeParams
    {
        public List<HedgeAsset> BuyAssets { get; set; } = new List<HedgeAsset>();
        public List<HedgeAsset> SellAssets { get; set; } = new List<HedgeAsset>();
        public decimal AmountPercent { get; set; }

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

        public class HedgeAsset
        {
            public decimal Weight { get; set; }
            public string Symbol { get; set; }
            public decimal NetBalanceInUsd { get; set; }
        }
    }
}