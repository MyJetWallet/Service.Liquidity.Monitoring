using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.Monitoring.Domain.Services;

namespace Service.Liquidity.Monitoring.Services
{
    public class AssetPortfolioStatusStorage : IAssetPortfolioStatusStorage, IStartable
    {
        private readonly ILogger<AssetPortfolioStatusStorage> _logger;
        private readonly IMyNoSqlServerDataWriter<AssetPortfolioVelocityStatusNoSql> _statusesDataWriter;
        private Dictionary<string, AssetPortfolioStatus> _statusMap = new Dictionary<string, AssetPortfolioStatus>();

        public AssetPortfolioStatusStorage(ILogger<AssetPortfolioStatusStorage> logger,
            IMyNoSqlServerDataWriter<AssetPortfolioVelocityStatusNoSql> statusesDataWriter)
        {
            _logger = logger;
            _statusesDataWriter = statusesDataWriter;
        }

        public AssetPortfolioStatus GetAssetPortfolioStatusByAsset(string asset)
        {
            return _statusMap.TryGetValue(asset, out var result) ? result : null;
        }

        public List<AssetPortfolioStatus> GetAssetPortfolioStatuses()
        {
            return _statusMap.Values.ToList();
        }

        public async Task UpdateAssetPortfolioStatusAsync(AssetPortfolioStatus assetPortfolioStatus)
        {
            await _statusesDataWriter.InsertOrReplaceAsync(AssetPortfolioVelocityStatusNoSql.Create(assetPortfolioStatus));

            await ReloadSettings();

            _logger.LogInformation("Updated AssetPortfolioStatus. StatusMap: {jsonText}",
                JsonConvert.SerializeObject(_statusMap));
        }

        private async Task ReloadSettings()
        {
            var statuses = (await _statusesDataWriter.GetAsync()).ToList();

            var statusMap = new Dictionary<string, AssetPortfolioStatus>();
            foreach (var status in statuses)
            {
                statusMap[status.AssetStatus.Asset] =
                    status.AssetStatus;
            }

            _statusMap = statusMap;
        }

        public void Start()
        {
            ReloadSettings().GetAwaiter().GetResult();
        }
    }
}