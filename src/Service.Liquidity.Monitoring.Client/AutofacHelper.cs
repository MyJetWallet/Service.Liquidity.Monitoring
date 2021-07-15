using Autofac;
using Service.Liquidity.Monitoring.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.Liquidity.Monitoring.Client
{
    public static class AutofacHelper
    {
        public static void MonitoringClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new MonitoringClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetHelloService()).As<IHelloService>().SingleInstance();
        }
    }
}
