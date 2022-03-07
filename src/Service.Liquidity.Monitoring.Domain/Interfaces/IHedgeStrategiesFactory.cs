using System.Collections.Generic;
using Service.Liquidity.Monitoring.Domain.Models.Hedging.Common;

namespace Service.Liquidity.Monitoring.Domain.Interfaces;

public interface IHedgeStrategiesFactory
{
    IEnumerable<IHedgeStrategy> Get();
    IHedgeStrategy Get(HedgeStrategyType type);
}