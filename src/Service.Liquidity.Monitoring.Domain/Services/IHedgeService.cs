using System.Threading.Tasks;

namespace Service.Liquidity.Monitoring.Domain.Services;

public interface IHedgeService
{
    public Task HedgeAsync();
}