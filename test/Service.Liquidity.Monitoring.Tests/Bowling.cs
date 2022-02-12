using System;
using System.Collections.Generic;
using NUnit.Framework;
using Service.Liquidity.Monitoring.Jobs;

namespace Service.Liquidity.Monitoring.Tests
{
    public class Bowling
    {
        
        [TestCase(-10, -5, 5, true)]
        [TestCase(-5, -5, 5, true)]
        [TestCase(-3, -5, 5, false)]
        [TestCase(3, -5, 5, false)]
        [TestCase(5, -5, 5, true)]
        [TestCase(10, -5, 5, true)]
        public void TestThresholdVelocity(decimal currValue, decimal min, decimal max, bool result)
        {
            var strike = CheckAssetPortfolioStatusBackgroundService.ThresholdVelocity(currValue, min, max);
            Assert.AreEqual(result, strike.IsAlarm);
        }

        [TestCase(-10000, -5000, true)]
        [TestCase(-5000, -5000, true)]
        [TestCase(0, -5000,  false)]
        [TestCase(5000, -5000,  false)]
        public void ThresholdTotalVelocityRisk(decimal currValue, decimal min, bool result)
        {
            var strike = CheckAssetPortfolioStatusBackgroundService.ThresholdMin(currValue, min);
            Assert.AreEqual(result, strike.IsAlarm);
        }
    }
}
