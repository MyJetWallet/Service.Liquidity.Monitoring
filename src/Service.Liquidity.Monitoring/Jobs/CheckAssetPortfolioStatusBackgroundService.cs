using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
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
        private const string FailUnicode = "\u2757";        
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

            List<AssetPortfolioStatus> isAlarmRiskAssets = new List<AssetPortfolioStatus>();
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
                
                _logger.LogInformation("Check asset {asset} last: {@lastAssetStatus} current: {@actualAssetStatus}", 
                    asset.Symbol, lastAssetStatus, actualAssetStatus);
                
                if (lastAssetStatus != null)
                {
                    if (lastAssetStatus.Velocity.IsAlarm != actualAssetStatus.Velocity.IsAlarm)
                    {
                        _logger.LogInformation("New velocity alert {status}", actualAssetStatus.ToJson());
                        await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(actualAssetStatus);
                        await PublishAssetStatusAsync(PrepareVelocityMessage(actualAssetStatus));
                    }
                
                    if(lastAssetStatus.VelocityRisk.IsAlarm != actualAssetStatus.VelocityRisk.IsAlarm)
                    {
                        _logger.LogInformation("New velocity risk alert {status}", actualAssetStatus.ToJson());
                        await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(actualAssetStatus);
                        await PublishAssetStatusAsync(PrepareVelocityRiskMessage(actualAssetStatus));
                    }

                    if (lastAssetStatus.VelocityRisk.IsAlarm)
                    {
                        isAlarmRiskAssets.Add(new AssetPortfolioStatus
                        {
                            Asset = lastAssetStatus.Asset,
                            Velocity = null,
                            VelocityRisk = new Status
                            {
                                ThresholdDate = lastAssetStatus.VelocityRisk.ThresholdDate,
                                CurrentValue = lastAssetStatus.VelocityRisk.CurrentValue,
                                ThresholdValue = lastAssetStatus.VelocityRisk.ThresholdValue,
                                IsAlarm = lastAssetStatus.VelocityRisk.IsAlarm
                            }
                        });
                    }
                }
                else
                {
                    _logger.LogInformation("Set default {asset} status {status}", asset.Symbol, actualAssetStatus.ToJson());
                    await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(actualAssetStatus);
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
                _logger.LogInformation("New total velocity risk alert {status}", actualTotalStatus.ToJson());
                await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(actualTotalStatus);
                await PublishAssetStatusAsync(PrepareTotalMessage(actualTotalStatus, isAlarmRiskAssets));
            }
        }

        //Velocity hit limit -5%
        //Current value: -5.5% 
        //Date: 2022-01-28 10:00:00
        private AssetPortfolioStatusMessage PrepareVelocityMessage(AssetPortfolioStatus actualAssetStatus)
        {
            var message = (actualAssetStatus.Velocity.IsAlarm
                ? $"{FailUnicode} {actualAssetStatus.Asset} <b>velocity</b> hit limit: {actualAssetStatus.Velocity.ThresholdValue}\r\n"
                : $"{SuccessUnicode} {actualAssetStatus.Asset} <b>velocity</b> back to normal\r\n") +
                  $"Current value: <b>{actualAssetStatus.Velocity.CurrentValue}</b>\r\n" +
                  $"Date: {actualAssetStatus.Velocity.ThresholdDate.ToString("yyyy-MM-dd hh:mm:ss")}";

            _logger.LogInformation("Prepare velocity message: {message}", message);
            
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

        private AssetPortfolioStatusMessage PrepareVelocityRiskMessage(AssetPortfolioStatus actualAssetStatus)
        {
            var message = (actualAssetStatus.VelocityRisk.IsAlarm
                ? $"{FailUnicode} {actualAssetStatus.Asset} <b>Alarm Net</b> hit limit {actualAssetStatus.VelocityRisk.ThresholdValue}\r\n"
                : $"{SuccessUnicode} {actualAssetStatus.Asset} <b>Alarm Net</b> back to normal\r\n") +
                  $"Current value: <b>{actualAssetStatus.VelocityRisk.CurrentValue}</b>\r\n" +
                  $"Date: {actualAssetStatus.VelocityRisk.ThresholdDate.ToString("yyyy-MM-dd hh:mm:ss")}";
           
            _logger.LogInformation("Prepare velocity risk message: {message}", message);
            
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
        
        // Exposure: Summary Alarm Net hit limit -20000$
        // Current value: -21000$ 
        // Assets:
        // BTC -12000$ [Limit: -6000$]
        // ETH -7000$ [Limit: -6000$]
        // ADA -3000$ [Limit: -3000$]
        // Date: 2022-01-28 10:00:00
        private AssetPortfolioStatusMessage PrepareTotalMessage(AssetPortfolioStatus actualAssetStatus,
            List<AssetPortfolioStatus> assetPortfolioStatusList)
        {
            var hitAssetsLines = string.Empty;
            foreach (var asset in assetPortfolioStatusList)
            {
                var messageline = ($"{FailUnicode} {asset.Asset} {asset.VelocityRisk.CurrentValue} " +
                                   $"[Limit: {asset.VelocityRisk.ThresholdValue}]\r\n");
                hitAssetsLines += messageline;
            }
            
            var message = (actualAssetStatus.VelocityRisk.IsAlarm
                              ? $"{FailUnicode} Exposure: <b>Summary Alarm Net</b> hit limit: {actualAssetStatus.VelocityRisk.ThresholdValue}\r\n"
                              : $"{SuccessUnicode} Exposure: <b>Summary Alarm Net</b> back to normal\r\n") +
                          $"Current value: <b>{actualAssetStatus.VelocityRisk.CurrentValue}</b>\r\n" +
                          $"Assets: \r\n{hitAssetsLines}" +    
                          $"Date: {actualAssetStatus.VelocityRisk.ThresholdDate.ToString("yyyy-MM-dd hh:mm:ss")}";

            _logger.LogInformation("Prepare Summary Alarm Net message: {message}", message);
            
            return new AssetPortfolioStatusMessage
            {
                Asset = AssetPortfolioSettingsNoSql.TotalSettingsAsset,
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

        private AssetPortfolioStatus GetActualStatusByAsset(Portfolio.Asset asset, 
            AssetPortfolioSettings assetSettingsByAsset)
        {
            if (asset.Symbol != assetSettingsByAsset.Asset && 
                AssetPortfolioSettingsNoSql.DefaultSettingsAsset != assetSettingsByAsset.Asset)
                throw new Exception("Bad asset settings");

            var velocityStatus =  ThresholdVelocity(Math.Round(asset.DailyVelocity, 2), Math.Round(assetSettingsByAsset.VelocityMin, 2), 
                Math.Round(assetSettingsByAsset.VelocityMax, 2));
            
            var velocityRiskStatus =  ThresholdVelocityRisk(Math.Round(asset.DailyVelocityRiskInUsd, 2),
                Math.Round(assetSettingsByAsset.VelocityRiskUsdMin, 2));
            
            var actualStatus = new AssetPortfolioStatus()
            {
                Asset = asset.Symbol,
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