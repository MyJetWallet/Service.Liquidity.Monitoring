using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models.Hedging.Common;

namespace Service.Liquidity.Monitoring.Domain.Interfaces;

public interface IHedgeService
{
    public Task HedgeAsync(HedgeParams hedgeParams);
}