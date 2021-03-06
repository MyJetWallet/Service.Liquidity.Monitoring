using Autofac;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Services;
using Service.Liquidity.Monitoring.Jobs;
using Service.Liquidity.Monitoring.NoSql.Backups;
using Service.Liquidity.Monitoring.NoSql.Rules;
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
                .RegisterType<PortfolioMonitoringJob>()
                .As<IStartable>()
                .SingleInstance()
                .AutoActivate();

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

            builder.RegisterType<MonitoringRulesNoSqlStorage>().As<IMonitoringRulesStorage>()
                .SingleInstance().AutoActivate();
            builder.RegisterType<MonitoringRulesBackupsNoSqlStorage>().As<IMonitoringRulesBackupsStorage>()
                .SingleInstance().AutoActivate();
        }
    }
}