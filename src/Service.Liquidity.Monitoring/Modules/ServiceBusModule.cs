using Autofac;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.Monitoring.Domain.Models.RuleSets;

namespace Service.Liquidity.Monitoring.Modules;

public class ServiceBusModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var serviceBusClient = builder.RegisterMyServiceBusTcpClient(
            Program.ReloadedSettings(e => e.SpotServiceBusHostPort),
            Program.LogFactory);
        
        builder.RegisterMyServiceBusPublisher<AssetPortfolioStatusMessage>(serviceBusClient,
            AssetPortfolioStatusMessage.TopicName, true);
        builder.RegisterMyServiceBusPublisher<MonitoringNotificationMessage>(serviceBusClient,
            MonitoringNotificationMessage.SbTopicName, true);
    }
}