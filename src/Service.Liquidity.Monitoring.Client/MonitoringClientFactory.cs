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
        
        public IAssetPortfolioSettingsManager GetAssetPortfolioSettingsService() => CreateGrpcService<IAssetPortfolioSettingsManager>();
    }
}
