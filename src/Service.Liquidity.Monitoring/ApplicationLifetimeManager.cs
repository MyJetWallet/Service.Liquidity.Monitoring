using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;
using Service.Liquidity.Monitoring.Jobs;

namespace Service.Liquidity.Monitoring
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly MyNoSqlTcpClient[] _myNoSqlTcpClientManagers;
        private readonly CheckAssetPortfolioStatusBackgroundService _checkAssetPortfolioStatusBackgroundService;

        public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime,
            ILogger<ApplicationLifetimeManager> logger,
            MyNoSqlTcpClient[] myNoSqlTcpClientManagers, 
            CheckAssetPortfolioStatusBackgroundService checkAssetPortfolioStatusBackgroundService)
            : base(appLifetime)
        {
            _logger = logger;
            _myNoSqlTcpClientManagers = myNoSqlTcpClientManagers;
            _checkAssetPortfolioStatusBackgroundService = checkAssetPortfolioStatusBackgroundService;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");
            _checkAssetPortfolioStatusBackgroundService.Start();
            foreach(var client in _myNoSqlTcpClientManagers)
            {
                client.Start();
            }
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");
            _checkAssetPortfolioStatusBackgroundService.Stop();
            foreach(var client in _myNoSqlTcpClientManagers)
            {
                try
                {
                    client.Stop();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");
        }
    }
}
