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
        private readonly TradingStrategyConfiguration _tradingStrategyConfiguration = new();
        private readonly TradingConfig _tradingConfig;

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
            _tradingConfig = tradingConfig ?? new(_tradingStrategyConfiguration.CurrencyDictionary, _tradingStrategyConfiguration.Symbol);
        }

        public async Task TradeOnLimitAsync()
        {
            try
            {
                while (true)
                {
                    var now = DateTime.UtcNow;
                    var getPriceTask = _binanceClient.GetPriceBySymbolAsync(_tradingConfig.Symbol);
                    var getKlinesTask = _binanceClient.GetKLinesBySymbolAsync(_tradingConfig.Symbol, _tradingConfig.Interval, _tradingConfig.Period.ToString());

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
                        await _tradeAction.Buy(_tradingConfig, currentCurrencyPrice, volatility, _tradingConfig.Symbol);
                    }

                    if (_tradingConfig.OpenPosition)
                    {
                        var (targetPrice, endProgram) = await _tradeAction.Sell(_tradingConfig, currentCurrencyPrice, volatility, _tradingConfig.Symbol);
                        if (endProgram)
                            break;

                        decimal targetBenefit = (_tradingConfig.TotalPurchaseCost + _tradingConfig.TargetProfit) - _tradingConfig.TotalPurchaseCost;

                        _logger.WriteLog($"timeElapsed {DateTime.UtcNow - now} | " +
                            $"diffMarge: {(targetPrice - _tradingConfig.CryptoPurchasePrice):F2} | " +
                            $"{_tradingConfig.Symbol}: {currentCurrencyPrice:F2} | " +
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
                            $"{_tradingConfig.Symbol}: {currentCurrencyPrice:F2} | " +
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
