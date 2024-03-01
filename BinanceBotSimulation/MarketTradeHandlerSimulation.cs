using BinanceBot.Abstraction;
using TradingCalculation;
using TradingCalculation.Strategy;

namespace BinanceBotSimulation;

public interface IMarketTradeHandlerSimulation
{
    Task CalculateProfit(decimal boughtPrice, decimal sellingPrice);
    Task CalculateMinimumMargeToProfit(decimal price);
}

public class MarketTradeHandlerSimulation(IPriceRetriever priceRetriever,
    ITechnicalIndicatorsCalculator technicalIndicatorsCalculator,
    ILogger logger) : IMarketTradeHandlerSimulation
{
    private readonly TradingStrategy _tradingStrategy = new()
    {
        TargetProfit = 300m,
        Quantity = 200m,
        Period = 600,
        Interval = "1s",
        Symbol = "SOLUSDT",
    };

    public async Task CalculateProfit(decimal boughtPrice, decimal sellingPrice)
    {
        try
        {
            await priceRetriever.HandleDiscountAsync(_tradingStrategy);
            
            decimal profit = technicalIndicatorsCalculator.CalculateProfit(
                boughtPrice,
                sellingPrice,
                _tradingStrategy.Quantity,
                _tradingStrategy.FeePercentage,
                _tradingStrategy.Discount);

            logger.WriteLog($"Profit for buy at {boughtPrice} and sell at {sellingPrice} | PROFIT = {profit}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur: {ex.Message}", ex);
            throw;
        }
    }

    public async Task CalculateMinimumMargeToProfit(decimal price)
    {
        try
        {
            await priceRetriever.HandleDiscountAsync(_tradingStrategy);

            decimal targetPriceFeesNotIncluded = ((price * _tradingStrategy.Quantity) + _tradingStrategy.TargetProfit) / _tradingStrategy.Quantity;
            decimal targetPriceFeesIncluded = targetPriceFeesNotIncluded * (1 + _tradingStrategy.FeePercentage);

            decimal forecastSellingPrice = technicalIndicatorsCalculator.CalculateMinimumSellingPrice(
                price,
                _tradingStrategy.Quantity,
                _tradingStrategy.FeePercentage,
                _tradingStrategy.Discount,
                _tradingStrategy.TargetProfit);

            logger.WriteLog($"diffMarge: {(forecastSellingPrice - price):F2} | " +
                $"forecastTargetPrice: {forecastSellingPrice:F2} | " +
                $"{_tradingStrategy.Symbol}: {price:F2} | " +
                $"targetPriceFeesIncluded: {targetPriceFeesIncluded:F2} | " +
                $"totalBenefit: {_tradingStrategy.TotalBenefit:F2} | " +
                $"quantity: {_tradingStrategy.Quantity}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur: {ex.Message}", ex);
            throw;
        }
    }
}
