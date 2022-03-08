using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models.Hedging.Common;

namespace Service.Liquidity.Monitoring.Domain.Services
{
    public class HedgeService : IHedgeService
    {
        public Task HedgeAsync(HedgeParams hedgeParams)
        {
            return Task.CompletedTask;
        }
    }
}