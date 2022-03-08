namespace Service.Liquidity.Monitoring.Domain.Models.Hedging.Common
{
    public class HedgeParams
    {
        public string SellAssetSymbol { get; set; }
        public string BuyAssetSymbol { get; set; }
        public decimal Amount { get; set; }
    }
}