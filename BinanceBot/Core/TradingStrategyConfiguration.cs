using BinanceBot.Model;

namespace BinanceBot.Core
{
    public class TradingStrategyConfiguration
    {
        public readonly string Symbol = "SOLUSDT";

        public readonly Dictionary<string, StrategyCurrencyConfiguration> CurrencyDictionary = new()
        {
            { "SOLUSDT", new StrategyCurrencyConfiguration { TargetProfit = 20m, Quantity = 100m, Interval = "1m", Period = 30 } },
            { "ETHUSDT", new StrategyCurrencyConfiguration { TargetProfit = 50m, Quantity = 15m, Interval = "1s", Period = 300 } },
            { "ADAUSDT", new StrategyCurrencyConfiguration { TargetProfit = 1m, Quantity = 2000m, Interval = "1s", Period = 60 } }
        };
    }
}
