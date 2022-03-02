using Autofac;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.Monitoring.Domain.Services;
using Service.Liquidity.Monitoring.Jobs;
using Service.Liquidity.Monitoring.NoSql.Checks;
using Service.Liquidity.Monitoring.NoSql.RuleSets;
using Service.Liquidity.Monitoring.Services;
using Service.Liquidity.TradingPortfolio.Domain.Models.NoSql;

namespace Service.Liquidity.Monitoring.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var noSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            builder.RegisterMyNoSqlWriter<AssetPortfolioSettingsNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), AssetPortfolioSettingsNoSql.TableName);
            builder.RegisterMyNoSqlWriter<AssetPortfolioStatusNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), AssetPortfolioStatusNoSql.TableName);
            builder.RegisterMyNoSqlReader<PortfolioNoSql>(noSqlClient, PortfolioNoSql.TableName);
            builder.RegisterMyNoSqlWriter<PortfolioCheckNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), PortfolioCheckNoSql.TableName);
            builder.RegisterMyNoSqlWriter<MonitoringRuleSetNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), MonitoringRuleSetNoSql.TableName);

            //todo: рассказать Леше =))
            builder
                .RegisterType<AssetPortfolioSettingsStorage>()
                .As<IAssetPortfolioSettingsStorage>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
            builder
                .RegisterType<AssetPortfolioStatusStorage>()
                .As<IAssetPortfolioStatusStorage>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<ExecuteRuleSetsJob>()
                .As<IStartable>()
                .SingleInstance()
                .AutoActivate();
            
            builder.RegisterType<CheckAssetPortfolioStatusBackgroundService>()
                .SingleInstance()
                .AutoActivate()
                .AsSelf();
            
            builder.RegisterType<PortfolioChecksNoSqlStorage>()
                .As<IPortfolioChecksStorage>()
                .SingleInstance()
                .AutoActivate()
                .AsSelf();
            builder.RegisterType<PortfolioMetricsFactory>()
                .As<IPortfolioMetricsFactory>()
                .SingleInstance()
                .AutoActivate()
                .AsSelf();
            builder.RegisterType<MonitoringRuleSetsNoSqlStorage>()
                .As<IMonitoringRuleSetsStorage>()
                .SingleInstance()
                .AutoActivate()
                .AsSelf(); 
            
            // Service Bus
            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(
                Program.ReloadedSettings(e => e.SpotServiceBusHostPort), 
                Program.LogFactory);
            
            //Publishers
            builder.RegisterMyServiceBusPublisher<AssetPortfolioStatusMessage>(serviceBusClient, AssetPortfolioStatusMessage.TopicName, true);
            builder.RegisterMyServiceBusPublisher<MonitoringNotificationMessage>(serviceBusClient, MonitoringNotificationMessage.SbTopicName, false);

        }
    }
}