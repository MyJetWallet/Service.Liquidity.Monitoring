using Autofac;
using OpenTelemetry.Context.Propagation;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Services;
using Service.Liquidity.Monitoring.Jobs;
using Service.Liquidity.Monitoring.NoSql.Checks;
using Service.Liquidity.Monitoring.NoSql.RuleSets;
using Service.Liquidity.Monitoring.Services;
using Service.Liquidity.TradingPortfolio.Client;

namespace Service.Liquidity.Monitoring.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterLiquidityTradingPortfolioClient(Program.Settings.LiquidityTradingPortfolioServiceUrl);
            
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
                .RegisterType<CheckPortfolioJob>()
                .As<IStartable>()
                .SingleInstance()
                .AutoActivate();

            builder.RegisterType<RefreshPortfolioStatusesJob>()
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

            builder
                .RegisterType<MonitoringRuleSetsMemoryCache>()
                .As<IMonitoringRuleSetsCache>()
                .AutoActivate()
                .SingleInstance()
                .AsSelf();
            builder
                .RegisterType<MonitoringRuleSetsExecutor>()
                .As<IMonitoringRuleSetsExecutor>()
                .AutoActivate()
                .SingleInstance()
                .AsSelf();
            builder
                .RegisterType<PortfolioChecksExecutor>()
                .As<IPortfolioChecksExecutor>()
                .AutoActivate()
                .SingleInstance()
                .AsSelf();
        }
    }
}