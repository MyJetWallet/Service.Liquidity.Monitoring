using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.Monitoring.Domain.Services;
using Service.Liquidity.TradingPortfolio.Client;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models.NoSql;

namespace Service.Liquidity.Monitoring.Jobs
{
    public class CheckAssetPortfolioStatusBackgroundService
    {
        private readonly ILogger<CheckAssetPortfolioStatusBackgroundService> _logger;
        private readonly IMyNoSqlServerDataReader<PortfolioNoSql> _myNoSqlServerDataReader;
        private readonly IAssetPortfolioSettingsStorage _assetPortfolioSettingsStorage;
        private readonly IAssetPortfolioStatusStorage _assetPortfolioStatusStorage;
        private readonly MyTaskTimer _operationsTimer;
#if DEBUG
        private const int TimerSpanSec = 30;
#else
        private const int TimerSpanSec = 60;
#endif    
        
        public CheckAssetPortfolioStatusBackgroundService(
            IMyNoSqlServerDataReader<PortfolioNoSql> myNoSqlServerDataReader,
            ILogger<CheckAssetPortfolioStatusBackgroundService> logger,
            IAssetPortfolioSettingsStorage assetPortfolioSettingsStorage,
            IAssetPortfolioStatusStorage assetPortfolioStatusStorage)
        {
            _myNoSqlServerDataReader = myNoSqlServerDataReader;
            _logger = logger;
            _assetPortfolioSettingsStorage = assetPortfolioSettingsStorage;
            _assetPortfolioStatusStorage = assetPortfolioStatusStorage;
            _operationsTimer = new MyTaskTimer(nameof(CheckAssetPortfolioStatusBackgroundService), 
                TimeSpan.FromSeconds(TimerSpanSec), logger, Process);
        }

        public void Start()
        {
            _operationsTimer.Start();
        }
        public void Stop()
        {
            _operationsTimer.Stop();
        }

        private async Task Process()
        {
            var portfolio = _myNoSqlServerDataReader.Get().FirstOrDefault();
            _logger.LogInformation("Get portfolio from PortfolioNoSql");
            await RefreshStatuses(portfolio?.Portfolio);
        }

        private async Task RefreshStatuses(Portfolio portfolio)
        {
            var assets = portfolio?.Assets;
            
            if (assets == null || !assets.Any())
            {
                _logger.LogError($"{PortfolioNoSql.TableName} is empty!!!");
                return;
            }
            // Check asset velocity and velocityRisk
            foreach (var asset in assets.Values)
            {
                
                var assetSettingsByAsset = _assetPortfolioSettingsStorage.GetAssetPortfolioSettingsByAsset(asset.Symbol);
                if (assetSettingsByAsset == null)
                {
                    assetSettingsByAsset = _assetPortfolioSettingsStorage.GetAssetPortfolioSettingsByAsset(AssetPortfolioSettingsNoSql.DefaultSettingsAsset);
                }
                if (assetSettingsByAsset == null)
                {
                    _logger.LogError($"Default settings not found in {AssetPortfolioSettingsNoSql.TableName}!!!");
                    return;
                }
                
                var lastAssetStatus = _assetPortfolioStatusStorage.GetAssetPortfolioStatusByAsset(asset.Symbol);
                var actualAssetStatus = GetActualStatusByAsset(asset, assetSettingsByAsset);

                if (lastAssetStatus == null || 
                    (lastAssetStatus.Velocity.IsAlarm != actualAssetStatus.Velocity.IsAlarm) ||
                    (lastAssetStatus.VelocityRisk.IsAlarm != actualAssetStatus.VelocityRisk.IsAlarm))
                {
                    await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(actualAssetStatus);
                }
            }
            // Check Total
            var assetSettingsByTotal = _assetPortfolioSettingsStorage.GetAssetPortfolioSettingsByAsset(AssetPortfolioSettingsNoSql.TotalSettingsAsset);
            if (assetSettingsByTotal == null)
            {
                _logger.LogError($"Total settings not found in {AssetPortfolioSettingsNoSql.TableName}!!!");
            }

            var lastTotalStatus = _assetPortfolioStatusStorage.GetAssetPortfolioStatusByAsset(AssetPortfolioSettingsNoSql.TotalSettingsAsset);
            var actualTotalStatus = GetActualStatusByTotal(assets.Values.ToList(), assetSettingsByTotal);

            if (lastTotalStatus == null || 
                lastTotalStatus.VelocityRisk.IsAlarm != actualTotalStatus.VelocityRisk.IsAlarm)
            {
                await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(actualTotalStatus);
            }
        }
        
        private AssetPortfolioStatus GetActualStatusByTotal(List<Portfolio.Asset> assetBalances, 
            AssetPortfolioSettings assetSettingsByAsset)
        {
            var totalVelocityRiskInUsd = assetBalances.Sum(asset => asset.DailyVelocityRiskInUsd);
            
            var actualStatus = new AssetPortfolioStatus()
            {
                Asset = AssetPortfolioSettingsNoSql.TotalSettingsAsset,
                VelocityRisk = ThresholdTotalVelocityRisk(totalVelocityRiskInUsd, 
                    assetSettingsByAsset.VelocityRiskUsdMin)
            };
            return actualStatus;
        }

        private AssetPortfolioStatus GetActualStatusByAsset(Portfolio.Asset assetBalance, 
            AssetPortfolioSettings assetSettingsByAsset)
        {
            if (assetBalance.Symbol != assetSettingsByAsset.Asset && 
                AssetPortfolioSettingsNoSql.DefaultSettingsAsset != assetSettingsByAsset.Asset)
                throw new Exception("Bad asset settings");

            var velocityStatus =  ThresholdVelocity(assetBalance.DailyVelocity, assetSettingsByAsset.VelocityMin, 
                assetSettingsByAsset.VelocityMax);
            var velocityRiskStatus =  ThresholdVelocityRisk(assetBalance.DailyVelocityRiskInUsd,
                assetSettingsByAsset.VelocityRiskUsdMin);
            
            var actualStatus = new AssetPortfolioStatus()
            {
                Asset = assetBalance.Symbol,
                Velocity = velocityStatus,
                VelocityRisk = velocityRiskStatus,

            };
            return actualStatus;
        }

        public static Status ThresholdVelocity(decimal value, decimal min, decimal max)
        {
            if (value <= min)
            {
                return new Status
                {
                    ThresholdDate = DateTime.UtcNow,
                    CurrentValue = value,
                    ThresholdValue = min,
                    IsAlarm = true
                };
            }
            
            if (value >= max)
            {
                return new Status
                {
                    ThresholdDate = DateTime.UtcNow,
                    CurrentValue = value,
                    ThresholdValue = max,
                    IsAlarm = true
                };
            }
            
            return new Status
            {
                ThresholdDate = DateTime.UtcNow,
                CurrentValue = value,
                ThresholdValue = value < 0 ? min : max,
                IsAlarm = false
            };
        }

        public static Status ThresholdVelocityRisk(decimal value, decimal min)
        {
            if (value <= min)
            {
                return new Status
                {
                    ThresholdDate = DateTime.UtcNow,
                    CurrentValue = value,
                    ThresholdValue = min,
                    IsAlarm = true
                };
            }
            
            return new Status
            {
                ThresholdDate = DateTime.UtcNow,
                CurrentValue = value,
                ThresholdValue = min,
                IsAlarm = false
            };
        }        
        
        public static Status ThresholdTotalVelocityRisk(decimal value, decimal min)
        {
            if (value <= min)
            {
                return new Status
                {
                    ThresholdDate = DateTime.UtcNow,
                    CurrentValue = value,
                    ThresholdValue = min,
                    IsAlarm = true
                };
            }
            
            return new Status
            {
                ThresholdDate = DateTime.UtcNow,
                CurrentValue = value,
                ThresholdValue = min,
                IsAlarm = false
            };
        } 
    }
}