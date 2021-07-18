using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.NoSql;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.Monitoring.Domain.Services;
using Service.Liquidity.Monitoring.Services;

namespace Service.Liquidity.Monitoring.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMyNoSqlWriter<AssetPortfolioSettingsNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), AssetPortfolioSettingsNoSql.TableName);
            
            builder
                .RegisterType<AssetPortfolioSettingsStorage>()
                .As<IAssetPortfolioSettingsStorage>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}