using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using MyJetWallet.Sdk.ServiceBus;
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
        private IServiceBusPublisher<AssetPortfolioStatusMessage> _assetPortfolioStatusPublisher;
#if DEBUG
        private const int TimerSpanSec = 30;
#else
        private const int TimerSpanSec = 60;
#endif    
        private const string SuccessUnicode = "👍";
        private const string FailUnicode = "👎🏿";        
        public CheckAssetPortfolioStatusBackgroundService(
            IMyNoSqlServerDataReader<PortfolioNoSql> myNoSqlServerDataReader,
            ILogger<CheckAssetPortfolioStatusBackgroundService> logger,
            IAssetPortfolioSettingsStorage assetPortfolioSettingsStorage,
            IAssetPortfolioStatusStorage assetPortfolioStatusStorage, 
            IServiceBusPublisher<AssetPortfolioStatusMessage> assetPortfolioStatusPublisher)
        {
            _myNoSqlServerDataReader = myNoSqlServerDataReader;
            _logger = logger;
            _assetPortfolioSettingsStorage = assetPortfolioSettingsStorage;
            _assetPortfolioStatusStorage = assetPortfolioStatusStorage;
            _assetPortfolioStatusPublisher = assetPortfolioStatusPublisher;
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
                
                var assetSettingsByAsset = await _assetPortfolioSettingsStorage.GetAssetPortfolioSettingsByAsset(asset.Symbol);
                if (assetSettingsByAsset == null)
                {
                    assetSettingsByAsset = await _assetPortfolioSettingsStorage.GetAssetPortfolioSettingsByAsset(AssetPortfolioSettingsNoSql.DefaultSettingsAsset);
                }
                if (assetSettingsByAsset == null)
                {
                    _logger.LogError($"Default settings not found in {AssetPortfolioSettingsNoSql.TableName}!!!");
                    return;
                }
                
                var lastAssetStatus = _assetPortfolioStatusStorage.GetAssetPortfolioStatusByAsset(asset.Symbol);
                var actualAssetStatus = GetActualStatusByAsset(asset, assetSettingsByAsset);

                if (lastAssetStatus != null)
                {
                    if (lastAssetStatus.Velocity.IsAlarm != actualAssetStatus.Velocity.IsAlarm)
                    {
                        await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(actualAssetStatus);
                        await PublishAssetStatusAsync(PrepareVelosityMessage(actualAssetStatus));
                    }
                
                    if(lastAssetStatus.VelocityRisk.IsAlarm != actualAssetStatus.VelocityRisk.IsAlarm)
                    {
                        await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(actualAssetStatus);
                        await PublishAssetStatusAsync(PrepareVelosityRiskMessage(actualAssetStatus));
                    }
                }
            }
            // Check Total
            var assetSettingsByTotal = await _assetPortfolioSettingsStorage.GetAssetPortfolioSettingsByAsset(AssetPortfolioSettingsNoSql.TotalSettingsAsset);
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

        //Velocity hit limit -5%
        //Current value: -5.5% 
        //Date: 2022-01-28 10:00:00
        private AssetPortfolioStatusMessage PrepareVelosityMessage(AssetPortfolioStatus actualAssetStatus)
        {

            var message = (actualAssetStatus.Velocity.IsAlarm
                ? $"{FailUnicode} {actualAssetStatus.Asset} velocity hit limit: {actualAssetStatus.Velocity.ThresholdValue}\r\n"
                : $"{SuccessUnicode} {actualAssetStatus.Asset} velocity back to normal\r\n") +
                  $"Current value: {actualAssetStatus.Velocity.CurrentValue}\r\n" +
                  $"Date: {actualAssetStatus.Velocity.ThresholdDate.ToString("yyyy-MM-dd hh:mm:ss")}";

            return new AssetPortfolioStatusMessage
            {
                Asset = actualAssetStatus.Asset,
                ThresholdDate = actualAssetStatus.Velocity.ThresholdDate,
                CurrentValue = actualAssetStatus.Velocity.CurrentValue,
                ThresholdValue = actualAssetStatus.Velocity.ThresholdValue,
                IsAlarm = actualAssetStatus.Velocity.IsAlarm,
                Message = message
            };
        }

        private AssetPortfolioStatusMessage PrepareVelosityRiskMessage(AssetPortfolioStatus actualAssetStatus)
        {
            var message = (actualAssetStatus.VelocityRisk.IsAlarm
                ? $"{actualAssetStatus.Asset} Alarm Net hit limit {actualAssetStatus.VelocityRisk.ThresholdValue}\r\n"
                : $"{actualAssetStatus.Asset} Alarm Net back to normal\r\n") +
                  $"Current value: {actualAssetStatus.VelocityRisk.CurrentValue}\r\n" +
                  $"Date: {actualAssetStatus.VelocityRisk.ThresholdDate.ToString("yyyy-MM-dd hh:mm:ss")}";

            return new AssetPortfolioStatusMessage
            {
                Asset = actualAssetStatus.Asset,
                ThresholdDate = actualAssetStatus.VelocityRisk.ThresholdDate,
                CurrentValue = actualAssetStatus.VelocityRisk.CurrentValue,
                ThresholdValue = actualAssetStatus.VelocityRisk.ThresholdValue,
                IsAlarm = actualAssetStatus.VelocityRisk.IsAlarm,
                Message = message
            };
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

            var velocityStatus =  ThresholdVelocity(Math.Round(assetBalance.DailyVelocity, 2), Math.Round(assetSettingsByAsset.VelocityMin, 2), 
                Math.Round(assetSettingsByAsset.VelocityMax, 2));
            
            var velocityRiskStatus =  ThresholdVelocityRisk(Math.Round(assetBalance.DailyVelocityRiskInUsd, 2),
                Math.Round(assetSettingsByAsset.VelocityRiskUsdMin, 2));
            
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
        
        private async Task PublishAssetStatusAsync(AssetPortfolioStatusMessage message)
        {
            await _assetPortfolioStatusPublisher.PublishAsync(message);
        }
    }

}