using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Monitoring.Domain.Models.Checks;

namespace Service.Liquidity.Monitoring.Domain.Interfaces
{
    public interface IPortfolioChecksStorage
    {
        Task<IEnumerable<PortfolioCheck>> GetAsync();
        Task AddOrUpdateAsync(PortfolioCheck model);
        Task<PortfolioCheck> GetAsync(string id);
        Task DeleteAsync(string id);
        Task BulkInsetOrUpdateAsync(IEnumerable<PortfolioCheck> models);
    }
}