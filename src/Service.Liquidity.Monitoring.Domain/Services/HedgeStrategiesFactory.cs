using System;
using System.Collections.Generic;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models.Hedging;
using Service.Liquidity.Monitoring.Domain.Models.Hedging.Common;

namespace Service.Liquidity.Monitoring.Domain.Services
{
    public class HedgeStrategiesFactory : IHedgeStrategiesFactory
    {
        private readonly Dictionary<HedgeStrategyType, IHedgeStrategy> _strategies;

        public HedgeStrategiesFactory()
        {
            _strategies = new Dictionary<HedgeStrategyType, IHedgeStrategy>
            {
                { HedgeStrategyType.CloseCollateralMaxVelocity, new CloseCollateralMaxVelocityHedgeStrategy() },
                { HedgeStrategyType.Return, new ReturnHedgeStrategy() },
                { HedgeStrategyType.None, new NoneHedgeStrategy() },
            };
        }

        public IEnumerable<IHedgeStrategy> Get()
        {
            return _strategies.Values;
        }

        public IHedgeStrategy Get(HedgeStrategyType type)
        {
            if (_strategies.TryGetValue(type, out var metric))
            {
                return metric;
            }

            throw new Exception($"Strategy {type.ToString()} Not Found");
        }
    }
}