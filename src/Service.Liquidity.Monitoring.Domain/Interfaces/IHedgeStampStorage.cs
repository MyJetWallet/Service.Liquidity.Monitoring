using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models.Hedging;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;

namespace Service.Liquidity.Monitoring.Domain.Interfaces;

public interface IHedgeStampStorage
{
    Task AddOrUpdateAsync(HedgeStamp model);
    Task<HedgeStamp> GetAsync();
}