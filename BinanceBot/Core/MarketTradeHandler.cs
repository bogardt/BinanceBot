using BinanceBot.Model;
using BinanceBot.Abstraction;

namespace BinanceBot.Core
{
    public class MarketTradeHandler : IMarketTradeHandler
    {
        private readonly IBinanceClient _binanceClient;
        private readonly IVolatilityStrategy _volatilityStrategy;
        private readonly ITechnicalIndicatorsCalculator _technicalIndicatorsCalculator;
        private readonly ITradeAction _tradeAction;
        private readonly ILogger _logger;

        private static readonly string _symbol = "SOLUSDT";

        private static readonly Dictionary<string, StrategyCurrencyConfiguration> _dict = new()
        {
            { "SOLUSDT", new StrategyCurrencyConfiguration { TargetProfit = 20m, Quantity = 10000m, Interval = "1s", Period = 10 } },
            { "ETHUSDT", new StrategyCurrencyConfiguration { TargetProfit = 50m, Quantity = 15m, Interval = "1s", Period = 300 } },
            { "ADAUSDT", new StrategyCurrencyConfiguration { TargetProfit = 1m, Quantity = 2000m, Interval = "1s", Period = 60 } }
        };

        private readonly TradingConfig _tradingConfig = new(_dict, _symbol);

        public MarketTradeHandler(IBinanceClient binanceClient,
            IVolatilityStrategy volatilityStrategy,
            ITechnicalIndicatorsCalculator technicalIndicatorsCalculator,
            ITradeAction tradeAction,
            ILogger logger,
            TradingConfig? tradingConfig = null)
        {
            _binanceClient = binanceClient;
            _volatilityStrategy = volatilityStrategy;
            _technicalIndicatorsCalculator = technicalIndicatorsCalculator;
            _tradeAction = tradeAction;
            _logger = logger;
            if (tradingConfig != null)
                _tradingConfig = tradingConfig;
        }

        public async Task TradeOnLimitAsync()
        {
            var lastTick = DateTime.UtcNow;
            try
            {
                while (true)
                {
                    var now = DateTime.UtcNow;
                    //if (now - lastTick > TimeSpan.FromMicroseconds(500))
                    lastTick = now;
                    var getPriceTask = _binanceClient.GetPriceBySymbolAsync(_symbol);
                    var getKlinesTask = _binanceClient.GetKLinesBySymbolAsync(_symbol, _tradingConfig.Interval, _tradingConfig.Period.ToString());

                    await Task.WhenAll(getPriceTask, getKlinesTask);

                    var currency = await getPriceTask;
                    var klines = await getKlinesTask;

                    decimal currentCurrencyPrice = currency.Price;

                    decimal mobileAverage = _technicalIndicatorsCalculator.CalculateMovingAverage(klines, periode: _tradingConfig.Period);
                    decimal rsi = _technicalIndicatorsCalculator.CalculateRSI(klines, periode: _tradingConfig.Period);
                    decimal volatility = _volatilityStrategy.CalculateVolatility(klines);

                    decimal targetPriceFeesNotIncluded = ((currentCurrencyPrice * _tradingConfig.Quantity) + _tradingConfig.TargetProfit) / _tradingConfig.Quantity;
                    decimal targetPriceFeesIncluded = targetPriceFeesNotIncluded * (1 + _tradingConfig.FeePercentage);

                    if (!_tradingConfig.OpenPosition && rsi <= _tradingConfig.MaxRSI && currentCurrencyPrice < mobileAverage)
                    {
                        await _tradeAction.Buy(_tradingConfig, currentCurrencyPrice, volatility, _symbol);
                    }

                    if (_tradingConfig.OpenPosition)
                    {
                        var (targetPrice, endProgram) = await _tradeAction.Sell(_tradingConfig, currentCurrencyPrice, volatility, _symbol);
                        if (endProgram)
                            break;

                        decimal targetBenefit = (_tradingConfig.TotalPurchaseCost + _tradingConfig.TargetProfit) - _tradingConfig.TotalPurchaseCost;

                        _logger.WriteLog($"timeElapsed {DateTime.UtcNow - now} | " +
                            $"diffMarge: {(targetPrice - _tradingConfig.CryptoPurchasePrice):F2} | " +
                            $"{_symbol}: {currentCurrencyPrice:F2} | " +
                            $"targetPrice: {targetPrice:F2} | " +
                            $"mobileAverage: {mobileAverage:F2} | " +
                            $"rsi: {rsi:F2} | " +
                            $"targetBenefit: {targetBenefit:F2} | " +
                            $"totalBenefit: {_tradingConfig.TotalBenefit:F2} | " +
                            $"quantity: {_tradingConfig.Quantity} | " +
                            $"volatility: {volatility:F2}");
                    }
                    else
                    {
                        _logger.WriteLog($"timeElapsed: {DateTime.UtcNow - now} | " +
                            $"{_symbol}: {currentCurrencyPrice:F2} | " +
                            $"targetPriceFeesNotIncluded: {targetPriceFeesNotIncluded:F2} | " +
                            $"targetPriceFeesIncluded: {targetPriceFeesIncluded:F2} | " +
                            $"mobileAverage: {mobileAverage:F2} | rsi : {rsi:F2} | " +
                            $"totalBenefit: {_tradingConfig.TotalBenefit:F2} | " +
                            $"quantity:  {_tradingConfig.Quantity} | " +
                            $"volatility: {volatility:F2}");
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}", ex);
                throw;
            }
        }
    }
}
