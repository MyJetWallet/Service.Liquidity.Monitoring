using System;
using NUnit.Framework;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.Monitoring.Jobs;

namespace Service.Liquidity.Monitoring.Tests
{
    public class AlarmNotifyTest
    {
        [Test]
        public void NormalToAlarm()
        {
            var last = new Status
            {
                ThresholdDate = DateTime.Now.AddHours(-1),
                CurrentValue = 5,
                ThresholdValue = 10,
                IsAlarm = false
            };
        
            var actual = new Status
            {
                ThresholdDate = DateTime.Now,
                CurrentValue = 11,
                ThresholdValue = 10,
                IsAlarm = true
            };
        
            var notify = RefreshPortfolioStatusesJob.IsStatusChanged(last, actual);
            Assert.AreEqual(true, notify);
        }
    
        [Test]
        public void AlarmToNormal()
        {
            var last = new Status
            {
                ThresholdDate = DateTime.Now.AddHours(-0.5),
                CurrentValue = 15,
                ThresholdValue = 10,
                IsAlarm = true
            };
        
            var actual = new Status
            {
                ThresholdDate = DateTime.Now,
                CurrentValue = 9,
                ThresholdValue = 10,
                IsAlarm = false
            };
        
            var notify = RefreshPortfolioStatusesJob.IsStatusChanged(last, actual);
            Assert.AreEqual(true, notify);
        }
    
        [Test]
        public void AlarmToAlarm1Minute()
        {
            var last = new Status
            {
                ThresholdDate = DateTime.Now.AddMinutes(-1),
                CurrentValue = 12,
                ThresholdValue = 10,
                IsAlarm = true
            };
        
            var actual = new Status
            {
                ThresholdDate = DateTime.Now,
                CurrentValue = 11,
                ThresholdValue = 10,
                IsAlarm = true
            };
        
            var notify = RefreshPortfolioStatusesJob.IsStatusChanged(last, actual);
            Assert.AreEqual(false, notify);
        }
    
        [Test]
        public void AlarmToAlarm1Hour()
        {
            var last = new Status
            {
                ThresholdDate = DateTime.Now.AddHours(-1),
                CurrentValue = 13,
                ThresholdValue = 10,
                IsAlarm = false
            };
        
            var actual = new Status
            {
                ThresholdDate = DateTime.Now,
                CurrentValue = 11,
                ThresholdValue = 10,
                IsAlarm = true
            };
        
            var notify = RefreshPortfolioStatusesJob.IsStatusChanged(last, actual);
            Assert.AreEqual(true, notify);
        }
    }
}