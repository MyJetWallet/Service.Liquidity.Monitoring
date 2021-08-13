using System;
using System.Collections.Generic;
using NUnit.Framework;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.Monitoring.Jobs;

namespace Service.Liquidity.Monitoring.Tests
{
    public class Bowling
    {
        private readonly AssetPortfolioSettings _settings = new AssetPortfolioSettings()
        {
            PositiveUpl = new List<decimal>()
            {
                {100m},
                {200m},
                {300m}
            }, 
            NegativeUpl = new List<decimal>()
            {
                {-100m},
                {-200m},
                {-300m}
            },
            PositiveNetUsd = new List<decimal>()
            {
                {100m},
                {200m},
                {300m}
            }, 
            NegativeNetUsd = new List<decimal>()
            {
                {-100m},
                {-200m},
                {-300m}
            }
        };
        [Test]
        public void Test1()
        {
            var strike = AssetPortfolioStateHandler.GetStrike(10, _settings.PositiveNetUsd, _settings.NegativeNetUsd);
            
            Assert.AreEqual(0, strike);
        }
        
        [Test]
        public void Test2()
        {
            var strike = AssetPortfolioStateHandler.GetStrike(150, _settings.PositiveNetUsd, _settings.NegativeNetUsd);
            
            Assert.AreEqual(100, strike);
        }
        
        [Test]
        public void Test2_1()
        {
            var strike = AssetPortfolioStateHandler.GetStrike(200, _settings.PositiveNetUsd, _settings.NegativeNetUsd);
            
            Assert.AreEqual(200, strike);
        }
        
        [Test]
        public void Test3()
        {
            var strike = AssetPortfolioStateHandler.GetStrike(350, _settings.PositiveNetUsd, _settings.NegativeNetUsd);
            
            Assert.AreEqual(300, strike);
        }
        
        [Test]
        public void Test4()
        {
            var strike = AssetPortfolioStateHandler.GetStrike(-10, _settings.PositiveNetUsd, _settings.NegativeNetUsd);
            
            Assert.AreEqual(0, strike);
        }
        
        [Test]
        public void Test5()
        {
            var strike = AssetPortfolioStateHandler.GetStrike(-250, _settings.PositiveNetUsd, _settings.NegativeNetUsd);
            
            Assert.AreEqual(-200, strike);
        }
        [Test]
        public void Test5_1()
        {
            var strike = AssetPortfolioStateHandler.GetStrike(-200, _settings.PositiveNetUsd, _settings.NegativeNetUsd);
            
            Assert.AreEqual(-200, strike);
        }
        
        [Test]
        public void Test6()
        {
            var strike = AssetPortfolioStateHandler.GetStrike(-350, _settings.PositiveNetUsd, _settings.NegativeNetUsd);
            
            Assert.AreEqual(-300, strike);
        }
    }
}
