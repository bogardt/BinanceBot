﻿using BinanceBot.Model;

namespace BinanceBot.Tests
{
    public static class TradeSetup
    {
        public static readonly Dictionary<string, StrategyCurrencyConfiguration> Dict = new()
        {
            { "SOLUSDT", new StrategyCurrencyConfiguration { TargetProfit = 10m, Quantity = 200m, Interval = "1m", Period = 60 } },
        };
        public static readonly string Symbol = "SOLUSDT";
    }
}
