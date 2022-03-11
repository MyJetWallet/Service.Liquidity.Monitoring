﻿using Autofac;
using MyNoSqlServer.Abstractions;
using Service.IndexPrices.Domain.Models;
using Service.Liquidity.Monitoring.Domain.Interfaces;

namespace Service.Liquidity.Monitoring.NoSql.Hedging;

public class CurrentPricesNoSqlCache : ICurrentPricesCache, IStartable
{
    private readonly IMyNoSqlServerDataReader<CurrentPricesNoSql> _reader;
    private Dictionary<string, CurrentPricesNoSql> _data = new();

    public CurrentPricesNoSqlCache(
        IMyNoSqlServerDataReader<CurrentPricesNoSql> reader
    )
    {
        _reader = reader;
    }

    public void Start()
    {
        _reader.SubscribeToUpdateEvents(models =>
        {
            lock (_data)
            {
                LoadData();
            }
        }, models =>
        {
            lock (_data)
            {
                LoadData();
            }
        });
    }

    public CurrentPrice Get(string source, string sourceMarket)
    {
        if (!_data.Any())
        {
            LoadData();
        }

        var key = GeneratePriceKey(source, sourceMarket);

        return _data.TryGetValue(key, out var pricesNoSql) ? pricesNoSql.Price : null;
    }

    private void LoadData()
    {
        _data = _reader.Get()?.ToDictionary(GeneratePriceKey) ?? new();
    }

    private string GeneratePriceKey(CurrentPricesNoSql priceNoSql)
    {
        return GeneratePriceKey(priceNoSql.Price.Source, priceNoSql.Price.SourceMarket);
    }

    private string GeneratePriceKey(string source, string sourceMarket)
    {
        return $"{source}-{sourceMarket}";
    }
}