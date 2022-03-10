using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.ExternalMarketApi;
using MyJetWallet.Domain.ExternalMarketApi.Dto;
using MyJetWallet.Domain.Orders;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models.Hedging.Common;

namespace Service.Liquidity.Monitoring.Domain.Services
{
    public class HedgeService : IHedgeService
    {
        private readonly ILogger<HedgeService> _logger;
        private readonly IExternalMarket _externalMarket;
        private const string ExchangeName = "FTX";

        public HedgeService(
            ILogger<HedgeService> logger,
            IExternalMarket externalMarket
        )
        {
            _logger = logger;
            _externalMarket = externalMarket;
        }

        public async Task HedgeAsync(HedgeParams hedgeParams)
        {
            var balancesResp = await _externalMarket.GetBalancesAsync(new GetBalancesRequest
            {
                ExchangeName = ExchangeName
            });
            var sellAsset = hedgeParams.SellAssets.First();
            var targetbalance = balancesResp.Balances
                .FirstOrDefault(b => b.Symbol == sellAsset.Symbol);
            // calculate amount in sell asset
            // check if has enough balance

            var tradeRequest = new MarketTradeRequest
            {
                Side = OrderSide.Buy,
                Market = $"{hedgeParams.BuyAssetSymbol}{sellAsset.Symbol}",
                Volume = Convert.ToDouble(hedgeParams.BuyVolume),
                ExchangeName = ExchangeName
            };
            //var tradeResp = await _externalMarket.MarketTrade(tradeRequest);

            // get possible trade pairs on exchange
            // find optimal trade
            // make market order
            // publish trade event
        }
    }
}