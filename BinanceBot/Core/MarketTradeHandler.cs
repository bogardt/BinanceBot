using BinanceBot.Abstraction;
using TradingCalculation;
using TradingCalculation.Strategy;

namespace BinanceBot.Core;

public class MarketTradeHandler(IExchangeHttpClient binanceClient,
    ITechnicalIndicatorsCalculator technicalIndicatorsCalculator,
    IPriceRetriever priceRetriever,
    ITradeAction tradeAction,
    ILogger logger,
    TradingStrategy? tradingStrategy = null) : IMarketTradeHandler
{
    private readonly TradingStrategy _tradingStrategy = tradingStrategy ?? new();

    public async Task TradeOnLimitAsync()
    {
        try
        {
            await priceRetriever.HandleDiscountAsync(_tradingStrategy);

            while (true)
            {
                var offSetPeriodFrom = 400;
                var offSetPeriodTo = 40;
                var offSetPeriod = offSetPeriodFrom - offSetPeriodTo;
                var period = _tradingStrategy.Period;// + offSetPeriod;

                var now = DateTime.UtcNow;
                var getPriceTask = binanceClient.GetPriceBySymbolAsync(_tradingStrategy.Symbol);
                var getKlinesTask = binanceClient.GetKLinesBySymbolAsync(_tradingStrategy.Symbol, _tradingStrategy.Interval, _tradingStrategy.Period.ToString());
                //var getKlinesTaskBefore = binanceClient.GetKLinesBySymbolAsync(DateTime.Now.AddMinutes((-1) * offSetPeriodFrom), DateTime.Now.AddMinutes((-1) * offSetPeriodTo), _tradingStrategy.Symbol, _tradingStrategy.Interval, period.ToString());

                await Task.WhenAll(getPriceTask, getKlinesTask);

                //await Task.WhenAll(getPriceTask, getKlinesTask, getKlinesTaskBefore);
                var currency = await getPriceTask;
                var klines = await getKlinesTask;
                //var klines = await getKlinesTaskBefore;
                //var actualKlines = await getKlinesTask;
                //klines.AddRange(actualKlines);

                decimal currentCurrencyPrice = (decimal)currency.Price!;

                var closingPrices = priceRetriever.GetClosingPrices(klines);

                decimal mobileAverage = technicalIndicatorsCalculator.CalculateMovingAverage(closingPrices, period);
                decimal rsi = technicalIndicatorsCalculator.CalculateRSI(closingPrices, period);
                decimal volatility = technicalIndicatorsCalculator.CalculateVolatility(closingPrices);

                decimal targetPriceFeesNotIncluded = ((currentCurrencyPrice * _tradingStrategy.Quantity) + _tradingStrategy.TargetProfit) / _tradingStrategy.Quantity;
                decimal targetPriceFeesIncluded = targetPriceFeesNotIncluded * (1 + _tradingStrategy.FeePercentage);
                decimal forecastSellingPrice = technicalIndicatorsCalculator.CalculateMinimumSellingPrice(currentCurrencyPrice, _tradingStrategy.Quantity, _tradingStrategy.FeePercentage, _tradingStrategy.Discount, _tradingStrategy.TargetProfit);

                var isAchievable = technicalIndicatorsCalculator.IsTargetPriceAchievable(forecastSellingPrice, closingPrices);

                if (!_tradingStrategy.OpenPosition && rsi <= _tradingStrategy.MaxRSI && currentCurrencyPrice < mobileAverage && isAchievable)
                {
                    await tradeAction.Buy(_tradingStrategy, currentCurrencyPrice, volatility, _tradingStrategy.Symbol);
                }

                var output = string.Empty;

                if (_tradingStrategy.OpenPosition)
                {
                    var (targetPrice, endProgram) = await tradeAction.Sell(_tradingStrategy, currentCurrencyPrice, volatility, _tradingStrategy.Symbol);
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

                logger.WriteLog($"{output} | " +
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
