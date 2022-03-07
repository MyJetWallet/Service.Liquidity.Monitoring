using System.Threading.Tasks;

namespace Service.Liquidity.Monitoring.Domain.Interfaces;

public interface IHedgeService
{
    public Task HedgeAsync();
}