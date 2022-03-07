using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.Service.Tools;
using MyJetWallet.Sdk.ServiceBus;
using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Extensions;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models.NoSql;

namespace Service.Liquidity.Monitoring.Jobs
{
    public class RefreshPortfolioStatusesJob
    {
        private readonly ILogger<RefreshPortfolioStatusesJob> _logger;
        private readonly IMyNoSqlServerDataReader<PortfolioNoSql> _myNoSqlServerDataReader;
        private readonly IAssetPortfolioSettingsStorage _assetPortfolioSettingsStorage;
        private readonly IAssetPortfolioStatusStorage _assetPortfolioStatusStorage;
        private readonly MyTaskTimer _operationsTimer;
        private IServiceBusPublisher<AssetPortfolioStatusMessage> _assetPortfolioStatusPublisher;

        private static readonly TimeSpan DefaultMonitorTimer = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan DefaultLastAlarmEventTimeSpan = TimeSpan.FromMinutes(60);
        private const string SuccessUnicode = "👍";
        private const string FailUnicode = "\u2757";        
        public RefreshPortfolioStatusesJob(
            IMyNoSqlServerDataReader<PortfolioNoSql> myNoSqlServerDataReader,
            ILogger<RefreshPortfolioStatusesJob> logger,
            IAssetPortfolioSettingsStorage assetPortfolioSettingsStorage,
            IAssetPortfolioStatusStorage assetPortfolioStatusStorage, 
            IServiceBusPublisher<AssetPortfolioStatusMessage> assetPortfolioStatusPublisher)
        {
            _myNoSqlServerDataReader = myNoSqlServerDataReader;
            _logger = logger;
            _assetPortfolioSettingsStorage = assetPortfolioSettingsStorage;
            _assetPortfolioStatusStorage = assetPortfolioStatusStorage;
            _assetPortfolioStatusPublisher = assetPortfolioStatusPublisher;
            _operationsTimer = new MyTaskTimer(nameof(RefreshPortfolioStatusesJob), 
                DefaultMonitorTimer, logger, DoAsync);
        }

        public void Start()
        {
            _operationsTimer.Start();
        }
        public void Stop()
        {
            _operationsTimer.Stop();
        }

        private async Task DoAsync()
        {
            var portfolio = _myNoSqlServerDataReader.Get().FirstOrDefault();
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
                
                if (lastAssetStatus != null)
                {
                    if (IsStatusChanged(lastAssetStatus.Velocity, actualAssetStatus.Velocity))
                    {
                        _logger.LogInformation("New velocity alert {status}", actualAssetStatus.ToJson());
                        await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(actualAssetStatus);
                        await PublishAssetStatusAsync(PrepareVelocityMessage(actualAssetStatus));
                    }
                
                    if(IsStatusChanged(lastAssetStatus.VelocityRisk, actualAssetStatus.VelocityRisk))
                    {
                        _logger.LogInformation("New velocity risk alert {status}", actualAssetStatus.ToJson());
                        await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(actualAssetStatus);
                        await PublishAssetStatusAsync(PrepareVelocityRiskMessage(actualAssetStatus));
                    }
                    
                    // Create total assets list
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
            await CheckTotalStatus(assets, isAlarmRiskAssets);
            await CheckTotalNegativeInUsdStatus(portfolio);
            await CheckTotalNegativeInPercentStatus(portfolio);
        }

        private async Task CheckTotalStatus(Dictionary<string, Portfolio.Asset> assets, List<AssetPortfolioStatus> isAlarmRiskAssets)
        {
            // Check Total
            var assetSettingsByTotal =
                await _assetPortfolioSettingsStorage.GetAssetPortfolioSettingsByAsset(AssetPortfolioSettingsNoSql
                    .TotalSettingsAsset);
            if (assetSettingsByTotal == null)
            {
                _logger.LogError($"Total settings not found in {AssetPortfolioSettingsNoSql.TableName}!!!");
            }

            var lastTotalStatus =
                _assetPortfolioStatusStorage.GetAssetPortfolioStatusByAsset(AssetPortfolioSettingsNoSql.TotalSettingsAsset);
            var actualTotalStatus = GetActualStatusByTotal(assets.Values.ToList(), assetSettingsByTotal);

            if (lastTotalStatus == null || 
                IsStatusChanged(lastTotalStatus.VelocityRisk, actualTotalStatus.VelocityRisk))
            {
                _logger.LogInformation("New total velocity risk alert {status}", actualTotalStatus.ToJson());
                await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(actualTotalStatus);
                await PublishAssetStatusAsync(PrepareTotalMessage(actualTotalStatus, isAlarmRiskAssets));
            }
        }
        
        private async Task CheckTotalNegativeInUsdStatus(Portfolio portfolio)
        {
            var name = AssetPortfolioSettingsNoSql.TotalNegativeInUsdSettingsAsset;
            var limitSettings =
                (await _assetPortfolioSettingsStorage.GetAssetPortfolioSettingsByAsset(name))?.TotalNegativeBalanceInUsdMin;
            
            if (limitSettings == null)
            {
                _logger.LogError($"Total negative $ settings not found in {AssetPortfolioSettingsNoSql.TableName}!!!");
            }

            var lastTotalStatus = _assetPortfolioStatusStorage.GetAssetPortfolioStatusByAsset(name);
            var actualTotalStatus = GetActualStatusByTotalNegativeInUsd(portfolio.TotalNegativeNetInUsd, limitSettings ?? 0m);

            if (lastTotalStatus == null ||
                IsStatusChanged(lastTotalStatus.NegativePositionInUSd, actualTotalStatus.NegativePositionInUSd))
            {
                _logger.LogInformation("New total negative $ risk alert {status}", actualTotalStatus.ToJson());
                await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(actualTotalStatus);
                await PublishAssetStatusAsync(PrepareTotalNegativeInUsdMessage(actualTotalStatus.NegativePositionInUSd,  name));
            }
        }
        
        private async Task CheckTotalNegativeInPercentStatus(Portfolio portfolio)
        {
            var name = AssetPortfolioSettingsNoSql.TotalNegativeInPercentSettingsAsset;
            var limitSettings =
                (await _assetPortfolioSettingsStorage.GetAssetPortfolioSettingsByAsset(name))?.TotalNegativeBalanceInPercent;
            
            if (limitSettings == null)
            {
                _logger.LogError($"Total negative % settings not found in {AssetPortfolioSettingsNoSql.TableName}!!!");
            }

            var lastTotalStatus = _assetPortfolioStatusStorage.GetAssetPortfolioStatusByAsset(name);
            var actualTotalStatus = GetActualStatusByTotalNegativeInPercent(portfolio.TotalNegativeNetPercent, limitSettings ?? 0m);

            if (lastTotalStatus == null ||
                IsStatusChanged(lastTotalStatus.NegativePositionInPercent, actualTotalStatus.NegativePositionInPercent))
            {
                _logger.LogInformation("New total negative % risk alert {status}", actualTotalStatus.ToJson());
                await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(actualTotalStatus);
                await PublishAssetStatusAsync(PrepareTotalNegativeInPercentMessage(actualTotalStatus.NegativePositionInPercent,  name));
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
            return actualAssetStatus.Velocity.PrepareMessage(actualAssetStatus.Asset, message);
        }

        private AssetPortfolioStatusMessage PrepareVelocityRiskMessage(AssetPortfolioStatus actualAssetStatus)
        {
            var message = (actualAssetStatus.VelocityRisk.IsAlarm
                ? $"{FailUnicode} {actualAssetStatus.Asset} <b>Alarm Net</b> hit limit {actualAssetStatus.VelocityRisk.ThresholdValue}\r\n"
                : $"{SuccessUnicode} {actualAssetStatus.Asset} <b>Alarm Net</b> back to normal\r\n") +
                  $"Current value: <b>{actualAssetStatus.VelocityRisk.CurrentValue}</b>\r\n" +
                  $"Date: {actualAssetStatus.VelocityRisk.ThresholdDate.ToString("yyyy-MM-dd hh:mm:ss")}";
           
            _logger.LogInformation("Prepare velocity risk message: {message}", message);
            return actualAssetStatus.VelocityRisk.PrepareMessage(actualAssetStatus.Asset, message);
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
            return actualAssetStatus.VelocityRisk.PrepareMessage(AssetPortfolioSettingsNoSql.TotalSettingsAsset, message);
        }
        
        private AssetPortfolioStatusMessage PrepareTotalNegativeInUsdMessage(Status actualAssetStatus, string asset)
        {
            var message = (actualAssetStatus.IsAlarm
                              ? $"{FailUnicode} <b>Negative position $</b> hit limit {actualAssetStatus.ThresholdValue}\r\n"
                              : $"{SuccessUnicode} <b>Negative position $</b> back to normal\r\n") +
                          $"Current value: <b>{actualAssetStatus.CurrentValue}</b>\r\n" +
                          $"Date: {actualAssetStatus.ThresholdDate.ToString("yyyy-MM-dd hh:mm:ss")}";
           
            _logger.LogInformation("Prepare negative position $ message: {message}", message);
            return actualAssetStatus.PrepareMessage(asset, message);
        } 
        
        private AssetPortfolioStatusMessage PrepareTotalNegativeInPercentMessage(Status actualAssetStatus, string asset)
        {
            var message = (actualAssetStatus.IsAlarm
                              ? $"{FailUnicode} <b>Negative position %</b> hit limit {actualAssetStatus.ThresholdValue}\r\n"
                              : $"{SuccessUnicode} <b>Negative position %</b> back to normal\r\n") +
                          $"Current value: <b>{actualAssetStatus.CurrentValue}</b>\r\n" +
                          $"Date: {actualAssetStatus.ThresholdDate.ToString("yyyy-MM-dd hh:mm:ss")}";
           
            _logger.LogInformation("Prepare negative position % message: {message}", message);
            return actualAssetStatus.PrepareMessage(asset, message);
        } 
        
        private AssetPortfolioStatus GetActualStatusByTotal(List<Portfolio.Asset> assetBalances, 
            AssetPortfolioSettings assetSettingsByAsset)
        {
            var totalVelocityRiskInUsd = assetBalances.Sum(asset => asset.DailyVelocityRiskInUsd);
            
            var actualStatus = new AssetPortfolioStatus()
            {
                Asset = AssetPortfolioSettingsNoSql.TotalSettingsAsset,
                VelocityRisk = ThresholdMin(totalVelocityRiskInUsd, 
                    assetSettingsByAsset.VelocityRiskUsdMin)
            };
            return actualStatus;
        }
        
        private AssetPortfolioStatus GetActualStatusByTotalNegativeInUsd(decimal currentValue, 
            decimal thresholdValue)
        {
            var actualStatus = new AssetPortfolioStatus()
            {
                Asset = AssetPortfolioSettingsNoSql.TotalNegativeInUsdSettingsAsset,
                NegativePositionInUSd = ThresholdMin(currentValue, 
                    thresholdValue)
            };
            return actualStatus;
        }
        
        private AssetPortfolioStatus GetActualStatusByTotalNegativeInPercent(decimal currentValue, 
            decimal thresholdValue)
        {
            var actualStatus = new AssetPortfolioStatus()
            {
                Asset = AssetPortfolioSettingsNoSql.TotalNegativeInPercentSettingsAsset,
                NegativePositionInPercent = ThresholdMax(currentValue, 
                    thresholdValue)
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
            
            var velocityRiskStatus =  ThresholdMin(Math.Round(asset.DailyVelocityRiskInUsd, 2),
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

        public static Status ThresholdMin(decimal value, decimal min)
        {
            return new Status
            {
                ThresholdDate = DateTime.UtcNow,
                CurrentValue = value,
                ThresholdValue = min,
                IsAlarm = value <= min
            };
        }        
        
        public static Status ThresholdMax(decimal value, decimal max)
        {
            return new Status
            {
                ThresholdDate = DateTime.UtcNow,
                CurrentValue = value,
                ThresholdValue = max,
                IsAlarm = value >= max
            };
        }
        
        private async Task PublishAssetStatusAsync(AssetPortfolioStatusMessage message)
        {
            await _assetPortfolioStatusPublisher.PublishAsync(message);
        }

        public static bool IsStatusChanged(Status last, Status actual)
        {
            var isStatusChanged = last.IsAlarm != actual.IsAlarm;
            var isStatusStillAlarm = last.IsAlarm && 
                                     actual.IsAlarm &&
                                     (DateTime.UtcNow - last.ThresholdDate > DefaultLastAlarmEventTimeSpan);
            
            return (isStatusChanged || isStatusStillAlarm);
        }
    }
    
    
}