using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models.Checks;

namespace Service.Liquidity.Monitoring.Domain.Services
{
    public interface IPortfolioCheckStorage
    {
        Task<IEnumerable<PortfolioCheck>> GetAsync();
        Task AddOrUpdateAsync(PortfolioCheck model);
        Task<PortfolioCheck> GetAsync(string id);
        Task DeleteAsync(string id);
    }
}