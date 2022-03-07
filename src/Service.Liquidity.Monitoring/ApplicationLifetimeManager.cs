using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyNoSqlServer.DataReader;
using Service.Liquidity.Monitoring.Jobs;

namespace Service.Liquidity.Monitoring
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly RefreshPortfolioStatusesJob _refreshPortfolioStatusesJob;
        private readonly ServiceBusLifeTime _myServiceBusTcpClient;
        private readonly MyNoSqlClientLifeTime _myNoSqlClientLifeTime;

        public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime,
            ILogger<ApplicationLifetimeManager> logger,
            MyNoSqlTcpClient[] myNoSqlTcpClientManagers, 
            RefreshPortfolioStatusesJob refreshPortfolioStatusesJob, 
            ServiceBusLifeTime myServiceBusTcpClient, 
            MyNoSqlClientLifeTime myNoSqlClientLifeTime)
            : base(appLifetime)
        {
            _logger = logger;
            _refreshPortfolioStatusesJob = refreshPortfolioStatusesJob;
            _myServiceBusTcpClient = myServiceBusTcpClient;
            _myNoSqlClientLifeTime = myNoSqlClientLifeTime;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");
            _myNoSqlClientLifeTime.Start();
            _refreshPortfolioStatusesJob.Start();
            _myServiceBusTcpClient.Start();
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");
            _myServiceBusTcpClient.Stop();
            _refreshPortfolioStatusesJob.Stop();
            _myNoSqlClientLifeTime.Stop();
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");
        }
    }
}
