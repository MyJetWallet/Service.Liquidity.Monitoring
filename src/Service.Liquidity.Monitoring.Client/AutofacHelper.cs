using Autofac;
using Service.Liquidity.Monitoring.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.Liquidity.Monitoring.Client
{
    public static class AutofacHelper
    {
        public static void MonitoringClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
        }

        public static void RegisterLiquidityMonitoringClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new MonitoringClientFactory(grpcServiceUrl);
            builder.RegisterInstance(factory.GetPortfolioChecksManager()).As<IPortfolioChecksManager>().SingleInstance();
            builder.RegisterInstance(factory.GetMonitoringRuleSetsManager()).As<IMonitoringRuleSetsManager>().SingleInstance();
            builder.RegisterInstance(factory.GetMonitoringRulesManager()).As<IMonitoringRulesManager>().SingleInstance();
        }
  
        public static void RegisterPortfolioChecksClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new MonitoringClientFactory(grpcServiceUrl);
            builder.RegisterInstance(factory.GetPortfolioChecksManager()).As<IPortfolioChecksManager>().SingleInstance();
        }
        
        public static void RegisterMonitoringRuleSetsClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new MonitoringClientFactory(grpcServiceUrl);
            builder.RegisterInstance(factory.GetMonitoringRuleSetsManager()).As<IMonitoringRuleSetsManager>().SingleInstance();
        }
    }
}
