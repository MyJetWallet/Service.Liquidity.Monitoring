using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.ExternalMarketApi;
using MyJetWallet.Domain.ExternalMarketApi.Dto;
using MyJetWallet.Domain.Orders;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models.Hedging;
using Service.Liquidity.Monitoring.Domain.Models.Hedging.Common;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Services
{
    public class HedgeService : IHedgeService
    {
        private readonly ILogger<HedgeService> _logger;
        private readonly IExternalMarket _externalMarket;
        private readonly IServiceBusPublisher<HedgeTradeMessage> _publisher;
        private readonly IHedgeStampStorage _hedgeStampStorage;
        private const string ExchangeName = "FTX";
        private static HedgeStamp _lastHedgeStamp;

        public HedgeService(
            ILogger<HedgeService> logger,
            IExternalMarket externalMarket,
            IServiceBusPublisher<HedgeTradeMessage> publisher,
            IHedgeStampStorage hedgeStampStorage
        )
        {
            _logger = logger;
            _externalMarket = externalMarket;
            _publisher = publisher;
            _hedgeStampStorage = hedgeStampStorage;
        }

        public async Task HedgeAsync(Portfolio portfolio, IEnumerable<MonitoringRuleSet> ruleSets)
        {
            _lastHedgeStamp ??= await _hedgeStampStorage.GetAsync();
            
            if (_lastHedgeStamp == null)
            {
                _lastHedgeStamp ??= new HedgeStamp();
                await _hedgeStampStorage.AddOrUpdateAsync(_lastHedgeStamp);
            }
            
            if (portfolio.HedgeStamp != null && portfolio.HedgeStamp < _lastHedgeStamp.Value)
            {
                _logger.LogWarning("Hedge is skipped. Portfolio hedge stamp less than last hedge stamp");
                return;
            }

            var hightestPriorityRule = ruleSets
                .Where(rs => rs.NeedsHedging())
                .SelectMany(rs => rs.Rules)
                .Where(rule => rule.CurrentState.HedgeParams.Validate(out _))
                .MaxBy(r => r.CurrentState.HedgeParams.BuyVolume);

            if (hightestPriorityRule == null)
            {
                _logger.LogWarning("No rule for hedging");
                return;
            }

            if (!hightestPriorityRule.CurrentState.HedgeParams.Validate(out var errors))
            {
                _logger.LogWarning(
                    $"Hedging is skipped. Found Rule {hightestPriorityRule.Name} with invalid HedgeParams {string.Join(", ", errors)}");
                return;
            }

            await TradeAsync(hightestPriorityRule.CurrentState.HedgeParams);
        }

        private async Task TradeAsync(HedgeParams hedgeParams)
        {
            var balancesResp = await _externalMarket.GetBalancesAsync(new GetBalancesRequest
            {
                ExchangeName = ExchangeName
            });
            var sellAsset = hedgeParams.SellAssets.First();
            var targetbalance = balancesResp.Balances
                .FirstOrDefault(b => b.Symbol == sellAsset.Symbol);

            // get possible trade pairs on exchange
            // find optimal trade
            // calculate amount in sell asset
            // check if has enough balance

            var tradeRequest = new MarketTradeRequest
            {
                Side = OrderSide.Buy,
                Market = $"{hedgeParams.BuyAssetSymbol}{sellAsset.Symbol}",
                Volume = Convert.ToDouble(hedgeParams.BuyVolume),
                ExchangeName = ExchangeName,
                OppositeVolume = 0,
                ReferenceId = null,
            };
            
            //var tradeResp = await _externalMarket.MarketTrade(tradeRequest);
            
            _lastHedgeStamp.Increase();
            await _publisher.PublishAsync(new HedgeTradeMessage
            {
                BaseAsset = hedgeParams.BuyAssetSymbol,
                BaseVolume = 0, // hedgeParams.BuyVolume,
                ExchangeName = ExchangeName,
                HedgeStamp = _lastHedgeStamp.Value,
                QuoteAsset = sellAsset.Symbol,
                QuoteVolume = 0
            });
            await _hedgeStampStorage.AddOrUpdateAsync(_lastHedgeStamp);

        }
    }
}