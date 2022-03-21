using MyNoSqlServer.Abstractions;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Domain.Models.Rules;

namespace Service.Liquidity.Monitoring.NoSql.RuleSets;

public class MonitoringRuleSetsMemoryCache : IMonitoringRuleSetsCache
{
    private readonly IMyNoSqlServerDataReader<MonitoringRuleSetNoSql> _reader;
    private Dictionary<string, MonitoringRuleSetNoSql> _data;

    public MonitoringRuleSetsMemoryCache(
        IMyNoSqlServerDataReader<MonitoringRuleSetNoSql> reader
    )
    {
        _reader = reader;
        _data = reader.Get()?.ToDictionary(m => m.Value.Id) ?? new();
        reader.SubscribeToUpdateEvents(models =>
        {
            lock (_data)
            {
                _data = _reader.Get().ToDictionary(d => d.Value.Id);
            }
        }, models =>
        {
            lock (_data)
            {
                _data = _reader.Get().ToDictionary(d => d.Value.Id);
            }
        });
    }

    public IEnumerable<MonitoringRuleSet> Get()
    {
        return _data.Any()
            ? _data.Values.Select(v => v.Value)
            : _reader.Get().Select(m => m.Value);
    }
}