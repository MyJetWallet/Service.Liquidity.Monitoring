using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.Liquidity.Monitoring.Grpc;

namespace Service.Liquidity.Monitoring.Client
{
    [UsedImplicitly]
    public class MonitoringClientFactory: MyGrpcClientFactory
    {
        public MonitoringClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }
        
        public IPortfolioChecksManager GetPortfolioChecksManager() => CreateGrpcService<IPortfolioChecksManager>();
        public IMonitoringRuleSetsManager GetMonitoringRuleSetsManager() => CreateGrpcService<IMonitoringRuleSetsManager>();
        public IMonitoringRulesManager GetMonitoringRulesManager() => CreateGrpcService<IMonitoringRulesManager>();
        public IMonitoringRulesBackupsManager GetMonitoringRulesBackupsManager() => CreateGrpcService<IMonitoringRulesBackupsManager>();

    }
}
