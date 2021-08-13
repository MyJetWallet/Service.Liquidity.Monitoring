using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.Monitoring.Domain.Services;
using Service.Liquidity.Portfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Jobs
{
    public class AssetPortfolioStateHandler : IStartable
    {
        private readonly ILogger<AssetPortfolioStateHandler> _logger;
        private readonly IMyNoSqlServerDataReader<AssetPortfolioBalanceNoSql> _myNoSqlServerDataReader;
        private readonly IAssetPortfolioSettingsStorage _assetPortfolioSettingsStorage;
        private readonly IAssetPortfolioStatusStorage _assetPortfolioStatusStorage;

        public AssetPortfolioStateHandler(IMyNoSqlServerDataReader<AssetPortfolioBalanceNoSql> myNoSqlServerDataReader,
            ILogger<AssetPortfolioStateHandler> logger,
            IAssetPortfolioSettingsStorage assetPortfolioSettingsStorage,
            IAssetPortfolioStatusStorage assetPortfolioStatusStorage)
        {
            _myNoSqlServerDataReader = myNoSqlServerDataReader;
            _logger = logger;
            _assetPortfolioSettingsStorage = assetPortfolioSettingsStorage;
            _assetPortfolioStatusStorage = assetPortfolioStatusStorage;
        }

        public void Start()
        {
            _myNoSqlServerDataReader.SubscribeToUpdateEvents(HandleUpdate, HandleDelete);
        }

        private void HandleDelete(IReadOnlyList<AssetPortfolioBalanceNoSql> balances)
        {
            _logger.LogInformation("Handle Delete message from AssetPortfolioBalanceNoSql");
        }

        private void HandleUpdate(IReadOnlyList<AssetPortfolioBalanceNoSql> balances)
        {
            _logger.LogInformation("Handle Update message from AssetPortfolioBalanceNoSql");

            RefreshStatuses(balances).GetAwaiter().GetResult();
        }

        private async Task RefreshStatuses(IReadOnlyList<AssetPortfolioBalanceNoSql> assetPortfolioBalance)
        {
            var balance = assetPortfolioBalance.FirstOrDefault()?.Balance;
            var assetBalances = balance?.BalanceByAsset;
            
            if (assetBalances == null || !assetBalances.Any())
            {
                _logger.LogError($"{AssetPortfolioBalanceNoSql.TableName} is empty!!!");
                return;
            }

            var someoneChanged = false;
            
            assetBalances.ForEach(async assetBalance =>
            {
                var assetSettingsByAsset = _assetPortfolioSettingsStorage.GetAssetPortfolioSettingsByAsset(assetBalance.Asset);
                if (assetSettingsByAsset == null)
                {
                    assetSettingsByAsset = _assetPortfolioSettingsStorage.GetAssetPortfolioSettingsByAsset(AssetPortfolioSettingsNoSql.DefaultSettingsAsset);
                }
                if (assetSettingsByAsset == null)
                {
                    _logger.LogError($"Default settings not found in {AssetPortfolioSettingsNoSql.TableName}!!!");
                    return;
                }
                var lastStatus = _assetPortfolioStatusStorage.GetAssetPortfolioStatusByAsset(assetBalance.Asset);
                var actualStatus = GetActualStatusByAsset(assetBalance, assetSettingsByAsset);

                if (lastStatus == null || 
                    lastStatus.UplStrike != actualStatus.UplStrike ||
                    lastStatus.NetUsdStrike != actualStatus.NetUsdStrike)
                {
                    someoneChanged = true;
                    await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(actualStatus);
                }
            });

            if (someoneChanged)
            {
                var assetSettingsByTotal = _assetPortfolioSettingsStorage.GetAssetPortfolioSettingsByAsset(AssetPortfolioSettingsNoSql.TotalSettingsAsset);
                if (assetSettingsByTotal == null)
                {
                    _logger.LogError($"Total settings not found in {AssetPortfolioSettingsNoSql.TableName}!!!");
                }

                var lastStatus = _assetPortfolioStatusStorage.GetAssetPortfolioStatusByAsset(AssetPortfolioSettingsNoSql.TotalSettingsAsset);
                var actualStatus = GetActualStatusByTotal(assetBalances, assetSettingsByTotal);

                if (lastStatus == null || 
                    lastStatus.UplStrike != actualStatus.UplStrike ||
                    lastStatus.NetUsdStrike != actualStatus.NetUsdStrike)
                {
                    someoneChanged = true;
                    await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(actualStatus);
                }
            }
        }
        
        private AssetPortfolioStatus GetActualStatusByTotal(List<NetBalanceByAsset> assetBalances, AssetPortfolioSettings assetSettingsByAsset)
        {
            var totalUpl = assetBalances.Sum(e => e.UnrealisedPnl);
            var totalNetUsd = assetBalances.Sum(e => e.NetUsdVolume);

            var uplStrike = GetStrike(totalUpl, assetSettingsByAsset.PositiveUpl, assetSettingsByAsset.NegativeUpl);
            var netUsdStrike = GetStrike(totalNetUsd, assetSettingsByAsset.PositiveNetUsd, assetSettingsByAsset.NegativeNetUsd);
            
            var actualStatus = new AssetPortfolioStatus()
            {
                Asset = AssetPortfolioSettingsNoSql.TotalSettingsAsset,
                UpdateDate = DateTime.UtcNow,
                UplStrike = uplStrike,
                NetUsdStrike = netUsdStrike,
                Upl = totalUpl,
                NetUsd = totalNetUsd
            };
            return actualStatus;
        }

        private AssetPortfolioStatus GetActualStatusByAsset(NetBalanceByAsset assetBalance, AssetPortfolioSettings assetSettingsByAsset)
        {
            if (assetBalance.Asset != assetSettingsByAsset.Asset && 
                AssetPortfolioSettingsNoSql.DefaultSettingsAsset != assetSettingsByAsset.Asset)
                throw new Exception("Bad asset settings");

            var uplStrike = GetStrike(assetBalance.UnrealisedPnl, assetSettingsByAsset.PositiveUpl, assetSettingsByAsset.NegativeUpl);
            var netUsdStrike = GetStrike(assetBalance.NetUsdVolume, assetSettingsByAsset.PositiveNetUsd, assetSettingsByAsset.NegativeNetUsd);
            
            var actualStatus = new AssetPortfolioStatus()
            {
                Asset = assetBalance.Asset,
                UpdateDate = DateTime.UtcNow,
                UplStrike = uplStrike,
                NetUsdStrike = netUsdStrike,
                Upl = assetBalance.UnrealisedPnl,
                NetUsd = assetBalance.NetUsdVolume
            };
            return actualStatus;
        }

        public static decimal GetStrike(decimal value, List<decimal> positiveArr, List<decimal> negativeArr)
        {
            if (value >= 0)
            {
                if (positiveArr == null || !positiveArr.Any())
                    return 0m;
                if (value < positiveArr.Min())
                    return 0m;
                if (value >= positiveArr.Max())
                    return positiveArr.Max();
                
                var strike = 0m;
                foreach (var positiveValue in positiveArr.OrderBy(e => e))
                {
                    if (value >= positiveValue)
                    {
                        strike = positiveValue;
                        continue;
                    }
                    return strike;
                }
                throw new Exception(
                    $"Something wrong in GetNetUsdStrike with value={value}" +
                    $" and positiveArr={JsonConvert.SerializeObject(positiveArr)}" +
                    $" and negativeArr={JsonConvert.SerializeObject(negativeArr)}");
            }
            else
            {
                if (negativeArr == null || !negativeArr.Any())
                    return 0m;
                if (value > negativeArr.Max())
                    return 0m;
                if (value <= negativeArr.Min())
                    return negativeArr.Min();

                var strike = 0m;
                foreach (var negativeValue in negativeArr.OrderByDescending(e => e))
                {
                    if (value <= negativeValue)
                    {
                        strike = negativeValue;
                        continue;
                    }
                    return strike;
                }
                throw new Exception(
                    $"Something wrong in GetNetUsdStrike with value={value}" +
                    $" and positiveArr={JsonConvert.SerializeObject(positiveArr)}" +
                    $" and negativeArr={JsonConvert.SerializeObject(negativeArr)}");
            }
        }
    }
}