using Autofac;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.Monitoring.Domain.Models;

namespace Service.Liquidity.Monitoring.Modules;

public class ServiceBusModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var serviceBusClient = builder.RegisterMyServiceBusTcpClient(
            () => Program.Settings.SpotServiceBusHostPort,
            Program.LogFactory);

        builder.RegisterMyServiceBusPublisher<PortfolioMonitoringMessage>(serviceBusClient,
            PortfolioMonitoringMessage.TopicName, false);
    }
}