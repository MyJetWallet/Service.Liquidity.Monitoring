using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.Monitoring.NoSql.Checks;
using Service.Liquidity.Monitoring.NoSql.RuleSets;
using Service.Liquidity.TradingPortfolio.Domain.Models.NoSql;

namespace Service.Liquidity.Monitoring.Modules;

public class NoSqlModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var noSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
        
        builder.RegisterMyNoSqlWriter<AssetPortfolioSettingsNoSql>(
            Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), AssetPortfolioSettingsNoSql.TableName);
        
        builder.RegisterMyNoSqlWriter<AssetPortfolioStatusNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
            AssetPortfolioStatusNoSql.TableName);
        
        builder.RegisterMyNoSqlReader<PortfolioNoSql>(noSqlClient, PortfolioNoSql.TableName);
        
        builder.RegisterMyNoSqlWriter<PortfolioCheckNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
            PortfolioCheckNoSql.TableName);
        
        builder.RegisterMyNoSqlWriter<MonitoringRuleSetNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
            MonitoringRuleSetNoSql.TableName);
        builder.RegisterMyNoSqlReader<MonitoringRuleSetNoSql>(noSqlClient, MonitoringRuleSetNoSql.TableName);
    }
}