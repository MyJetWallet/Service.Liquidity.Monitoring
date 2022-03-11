using Service.IndexPrices.Domain.Models;

namespace Service.Liquidity.Monitoring.Domain.Interfaces;

public interface ICurrentPricesCache
{
    CurrentPrice Get(string source, string sourceMarket);
}