using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Service.Liquidity.Portfolio.Domain.Models;

namespace Service.Liquidity.Monitoring.Jobs
{
    public class AssetPortfolioStateHandler : IStartable
    {
        private readonly ILogger<AssetPortfolioStateHandler> _logger;
        private readonly IMyNoSqlServerDataReader<AssetPortfolioBalanceNoSql> _myNoSqlServerDataReader;

        public AssetPortfolioStateHandler(IMyNoSqlServerDataReader<AssetPortfolioBalanceNoSql> myNoSqlServerDataReader,
            ILogger<AssetPortfolioStateHandler> logger)
        {
            _myNoSqlServerDataReader = myNoSqlServerDataReader;
            _logger = logger;
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
        }
    }
}