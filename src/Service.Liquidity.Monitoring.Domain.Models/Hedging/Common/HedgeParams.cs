using System.Collections.Generic;
using System.Linq;

namespace Service.Liquidity.Monitoring.Domain.Models.Hedging.Common
{
    public class HedgeParams
    {
        public List<BuyAsset> BuyAssets { get; set; } = new List<BuyAsset>();
        public List<SellAsset> SellAssets { get; set; } = new List<SellAsset>();

        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();

            if (!BuyAssets.Any())
            {
                errors.Add("Buy assets not found");
            }
            
            if (!SellAssets.Any())
            {
                errors.Add("Sell assets not found");
            }

            return !errors.Any();
        }

        public class SellAsset
        {
            public int Priority { get; set; }
            public string Symbol { get; set; }
            public decimal SellAmountInUsd { get; set; }
        }

        public class BuyAsset
        {
            public int Priority { get; set; }
            public string Symbol { get; set; }
            public decimal NetBalanceInUsd { get; set; }
        }
    }
}