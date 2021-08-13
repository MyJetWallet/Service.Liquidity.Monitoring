using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
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

            var x = _myNoSqlServerDataReader.Get();
            var y = x;
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

            var uplStrike = GetUplStrike(totalUpl, assetSettingsByAsset);
            var netUsdStrike = GetNetUsdStrike(totalNetUsd, assetSettingsByAsset);
            
            var actualStatus = new AssetPortfolioStatus()
            {
                Asset = AssetPortfolioSettingsNoSql.TotalSettingsAsset,
                UpdateDate = DateTime.UtcNow,
                UplStrike = uplStrike,
                NetUsdStrike = netUsdStrike
            };
            return actualStatus;
        }

        private AssetPortfolioStatus GetActualStatusByAsset(NetBalanceByAsset assetBalance, AssetPortfolioSettings assetSettingsByAsset)
        {
            if (assetBalance.Asset != assetSettingsByAsset.Asset)
                throw new Exception("Bad asset settings");

            var uplStrike = GetUplStrike(assetBalance.UnrealisedPnl, assetSettingsByAsset);
            var netUsdStrike = GetNetUsdStrike(assetBalance.NetUsdVolume, assetSettingsByAsset);
            
            var actualStatus = new AssetPortfolioStatus()
            {
                Asset = assetBalance.Asset,
                UpdateDate = DateTime.UtcNow,
                UplStrike = uplStrike,
                NetUsdStrike = netUsdStrike
            };
            return actualStatus;
        }

        private decimal GetNetUsdStrike(decimal assetBalanceNetUsdVolume, AssetPortfolioSettings assetSettingsByAsset)
        {
            if (assetBalanceNetUsdVolume >= 0)
            {
                foreach (var positiveNetUsd in assetSettingsByAsset.PositiveNetUsd.OrderBy(e => e))
                {
                    if (assetBalanceNetUsdVolume > positiveNetUsd)
                        continue;
                    return positiveNetUsd;
                }
                return assetSettingsByAsset.PositiveNetUsd.Max();
            }
            foreach (var negativeNetUsd in assetSettingsByAsset.NegativeNetUsd.OrderByDescending(e => e))
            {
                if (assetBalanceNetUsdVolume < negativeNetUsd)
                    continue;
                return negativeNetUsd;
            }
            return assetSettingsByAsset.NegativeNetUsd.Min();
        }

        private decimal GetUplStrike(decimal assetBalanceUnrealisedPnl, AssetPortfolioSettings assetSettingsByAsset)
        {
            if (assetBalanceUnrealisedPnl >= 0)
            {
                foreach (var positiveUpl in assetSettingsByAsset.PositiveUpl.OrderBy(e => e))
                {
                    if (assetBalanceUnrealisedPnl > positiveUpl)
                        continue;
                    return positiveUpl;
                }
                return assetSettingsByAsset.PositiveUpl.Max();
            }
            foreach (var negativeUpl in assetSettingsByAsset.NegativeUpl.OrderByDescending(e => e))
            {
                if (assetBalanceUnrealisedPnl < negativeUpl)
                    continue;
                return negativeUpl;
            }
            return assetSettingsByAsset.NegativeUpl.Min();
        }
    }
}