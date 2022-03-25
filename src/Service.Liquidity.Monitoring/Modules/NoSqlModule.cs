﻿using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.Liquidity.Monitoring.Domain.Models;
using Service.Liquidity.Monitoring.NoSql.Rules;
using Service.Liquidity.Monitoring.NoSql.RuleSets;
using Service.Liquidity.TradingPortfolio.Domain.Models.NoSql;

namespace Service.Liquidity.Monitoring.Modules;

public class NoSqlModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var noSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
        
        builder.RegisterMyNoSqlReader<PortfolioNoSql>(noSqlClient, PortfolioNoSql.TableName);

        builder.RegisterMyNoSqlWriter<MonitoringRuleSetNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
            MonitoringRuleSetNoSql.TableName);
        
        builder.RegisterMyNoSqlReader<MonitoringRuleSetNoSql>(noSqlClient, MonitoringRuleSetNoSql.TableName);
        
        builder.RegisterMyNoSqlWriter<MonitoringRuleNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
            MonitoringRuleNoSql.TableName);
    }
}