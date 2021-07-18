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

            RefreshAssetStatuses(balances).GetAwaiter().GetResult();
        }

        private async Task RefreshAssetStatuses(IReadOnlyList<AssetPortfolioBalanceNoSql> assetPortfolioBalance)
        {
            var balance = assetPortfolioBalance.FirstOrDefault()?.Balance;
            var assetBalances = balance?.BalanceByAsset;

            if (assetBalances == null || !assetBalances.Any())
            {
                _logger.LogError($"{AssetPortfolioBalanceNoSql.TableName} is empty!!!");
                return;
            }
            
            assetBalances.ForEach(async assetBalance =>
            {
                var assetSettingsByAsset = _assetPortfolioSettingsStorage.GetAssetPortfolioSettingsByAsset(assetBalance.Asset);
                if (assetSettingsByAsset == null)
                {
                    _logger.LogError($"Asset Settings for asset {assetBalance.Asset} not found in {AssetPortfolioSettingsNoSql.TableName}!!!");
                    return;
                }
                var lastStatus = _assetPortfolioStatusStorage.GetAssetPortfolioStatusByAsset(assetBalance.Asset);
                var actualStatus = GetActualStatus(assetBalance, assetSettingsByAsset);

                if (lastStatus == null || lastStatus.Status != actualStatus)
                {
                    await _assetPortfolioStatusStorage.UpdateAssetPortfolioStatusAsync(new AssetPortfolioStatus()
                    {
                        Asset = assetBalance.Asset,
                        Status = actualStatus
                    });
                }
            });
        }

        private AssetStatus GetActualStatus(NetBalanceByAsset assetBalance, AssetPortfolioSettings assetSettingsByAsset)
        {
            if (assetBalance.Asset != assetSettingsByAsset.Asset)
                throw new Exception("Bad asset settings");
            
            if (assetBalance.NetVolume < assetSettingsByAsset.NetWarningLevel)
            {
                return AssetStatus.Normal;
            }
            if (assetBalance.NetVolume < assetSettingsByAsset.NetDangerLevel)
            {
                return AssetStatus.Warning;
            }
            if (assetBalance.NetVolume < assetSettingsByAsset.NetCriticalLevel)
            {
                return AssetStatus.Danger;
            }
            return AssetStatus.Critical;
        }
    }
}