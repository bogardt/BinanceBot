using BinanceBot.Strategy;

namespace BinanceBot.Tests
{
    public static class TradeSetup
    {
        public static readonly TradingStrategy tradingStrategy = new()
        {
            TargetProfit = 10m,
            Quantity = 200m,
            Interval = "1m",
            Period = 60,
            Symbol = "SOLUSDT",
            LimitBenefit = 1000,
        };
    }
}
