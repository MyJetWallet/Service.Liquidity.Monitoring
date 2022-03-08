using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.ExternalMarketApi;
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

        public Task HedgeAsync(HedgeParams hedgeParams)
        {
            return Task.CompletedTask;
        }
    }
}