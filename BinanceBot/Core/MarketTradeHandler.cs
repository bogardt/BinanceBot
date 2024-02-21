using BinanceBot.Abstraction;
using BinanceBot.Strategy;

namespace BinanceBot.Core;

public class MarketTradeHandler(IBinanceClient binanceClient,
    ITechnicalIndicatorsCalculator technicalIndicatorsCalculator,
    IPriceRetriever priceRetriever,
    ITradeAction tradeAction,
    ILogger logger,
    TradingStrategy? tradingStrategy = null) : IMarketTradeHandler
{
    private readonly IBinanceClient _binanceClient = binanceClient;
    private readonly IPriceRetriever _priceRetriever = priceRetriever;
    private readonly ITechnicalIndicatorsCalculator _technicalIndicatorsCalculator = technicalIndicatorsCalculator;
    private readonly ITradeAction _tradeAction = tradeAction;
    private readonly ILogger _logger = logger;
    private readonly TradingStrategy _tradingStrategy = tradingStrategy ?? new();

    public async Task TradeOnLimitAsync()
    {
        try
        {
            await _priceRetriever.HandleDiscountAsync(_tradingStrategy);

            while (true)
            {
                var now = DateTime.UtcNow;
                var getPriceTask = _binanceClient.GetPriceBySymbolAsync(_tradingStrategy.Symbol);
                var getKlinesTask = _binanceClient.GetKLinesBySymbolAsync(_tradingStrategy.Symbol, _tradingStrategy.Interval, _tradingStrategy.Period.ToString());

                await Task.WhenAll(getPriceTask, getKlinesTask);

                var currency = await getPriceTask;
                var klines = await getKlinesTask;

                decimal currentCurrencyPrice = currency.Price;

                var closingPrices = _priceRetriever.GetClosingPrices(klines);

                decimal mobileAverage = _technicalIndicatorsCalculator.CalculateMovingAverage(closingPrices, _tradingStrategy.Period);
                decimal rsi = _technicalIndicatorsCalculator.CalculateRSI(closingPrices, _tradingStrategy.Period);
                decimal volatility = _technicalIndicatorsCalculator.CalculateVolatility(closingPrices);

                decimal targetPriceFeesNotIncluded = ((currentCurrencyPrice * _tradingStrategy.Quantity) + _tradingStrategy.TargetProfit) / _tradingStrategy.Quantity;
                decimal targetPriceFeesIncluded = targetPriceFeesNotIncluded * (1 + _tradingStrategy.FeePercentage);

                if (!_tradingStrategy.OpenPosition && rsi <= _tradingStrategy.MaxRSI && currentCurrencyPrice < mobileAverage)
                {
                    await _tradeAction.Buy(_tradingStrategy, currentCurrencyPrice, volatility, _tradingStrategy.Symbol);
                }

                var output = string.Empty;

                decimal forecastSellingPrice = _priceRetriever.CalculateMinimumSellingPrice(
                    currentCurrencyPrice,
                    _tradingStrategy.Quantity,
                    _tradingStrategy.FeePercentage,
                    _tradingStrategy.Discount,
                    _tradingStrategy.TargetProfit);

                if (_tradingStrategy.OpenPosition)
                {
                    var (targetPrice, endProgram) = await _tradeAction.Sell(_tradingStrategy, currentCurrencyPrice, volatility, _tradingStrategy.Symbol);
                    if (endProgram)
                        break;

                    //decimal targetBenefit = (_tradingStrategy.TotalPurchaseCost + _tradingStrategy.TargetProfit) - _tradingStrategy.CryptoPurchasePrice;

                    output += $"diffMarge: {(targetPrice - _tradingStrategy.CryptoPurchasePrice):F2} | ";
                    //output += $"targetBenefit: {targetBenefit:F2} | ";
                    output += $"targetPrice: {targetPrice:F2}";
                }
                else
                {
                    output += $"diffMarge: {(forecastSellingPrice - currentCurrencyPrice):F2} | ";
                    output += $"forecastTargetPrice: {forecastSellingPrice:F2}";
                }

                _logger.WriteLog($"{output} | " +
                    $"{_tradingStrategy.Symbol}: {currentCurrencyPrice:F2} | " +
                    //$"targetPriceFeesIncluded: {targetPriceFeesIncluded:F2} | " +
                    $"mobileAverage: {mobileAverage:F2} | " +
                    $"rsi: {rsi:F2} | " +
                    $"totalBenefit: {_tradingStrategy.TotalBenefit:F2} | " +
                    $"quantity: {_tradingStrategy.Quantity} | " +
                    $"volatility: {volatility:F2}");

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur: {ex.Message}", ex);
            throw;
        }
    }
}
