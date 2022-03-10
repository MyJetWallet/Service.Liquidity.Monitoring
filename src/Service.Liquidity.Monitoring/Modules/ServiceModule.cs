﻿using Autofac;
using MyJetWallet.Domain.ExternalMarketApi;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Services;
using Service.Liquidity.Monitoring.Jobs;
using Service.Liquidity.Monitoring.NoSql.Checks;
using Service.Liquidity.Monitoring.NoSql.Hedging;
using Service.Liquidity.Monitoring.NoSql.RuleSets;
using Service.Liquidity.Monitoring.Services;

namespace Service.Liquidity.Monitoring.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
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
                .RegisterType<HedgeStrategiesFactory>()
                .As<IHedgeStrategiesFactory>()
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
            builder
                .RegisterType<HedgeService>()
                .As<IHedgeService>()
                .AutoActivate()
                .SingleInstance()
                .AsSelf();
            builder.RegisterType<HedgeStampNoSqlStorage>().As<IHedgeStampStorage>().AutoActivate().SingleInstance()
                .AsSelf();


            builder.RegisterExternalMarketClient(Program.Settings.ExternalApiGrpcUrl);
        }
    }
}