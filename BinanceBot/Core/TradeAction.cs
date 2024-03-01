using BinanceBot.Abstraction;
using Newtonsoft.Json;
using TradingCalculation;
using TradingCalculation.Strategy;

namespace BinanceBot.Core;

public class TradeAction(
    IExchangeHttpClient binanceClient,
    ITechnicalIndicatorsCalculator technicalIndicatorsCalculator,
    ILogger logger) : ITradeAction
{
    public async Task Buy(TradingStrategy tradingStrategy, decimal currentCurrencyPrice, decimal volatility, string symbol)
    {
        tradingStrategy.CryptoPurchasePrice = currentCurrencyPrice;
        tradingStrategy.TotalPurchaseCost = tradingStrategy.Quantity * tradingStrategy.CryptoPurchasePrice;

        decimal feesAmount = tradingStrategy.TotalPurchaseCost * tradingStrategy.FeePercentage;
        tradingStrategy.TotalPurchaseCost *= (1 + tradingStrategy.FeePercentage);

        logger.WriteLog($"===> [ACHAT] {tradingStrategy.Quantity} | " +
            $"feesAmount: {feesAmount} | " +
            $"cryptoPurchasePrice: {tradingStrategy.CryptoPurchasePrice:F2} | " +
            $"totalPurchaseCost: {tradingStrategy.TotalPurchaseCost:F2}");

        if (tradingStrategy.TestMode)
        {
            var orderResponse = await binanceClient.PlaceTestOrderAsync(symbol, tradingStrategy.Quantity, currentCurrencyPrice, "BUY");
            logger.WriteLog($"test : {JsonConvert.SerializeObject(orderResponse, Formatting.Indented)}");
        }
        else
        {
            var orderResponse = await binanceClient.PlaceOrderAsync(symbol, tradingStrategy.Quantity, currentCurrencyPrice, "BUY");
            logger.WriteLog($"real : {JsonConvert.SerializeObject(orderResponse, Formatting.Indented)}");
        }

        await WaitBuyAsync(symbol);

        tradingStrategy.OpenPosition = true;
    }

    public async Task<(decimal, bool)> Sell(TradingStrategy tradingStrategy,
        decimal currentCurrencyPrice,
        decimal volatility,
        string symbol)
    {
        decimal prixVenteCible = technicalIndicatorsCalculator.CalculateMinimumSellingPrice(
            tradingStrategy.CryptoPurchasePrice,
            tradingStrategy.Quantity,
            tradingStrategy.FeePercentage,
            tradingStrategy.Discount,
            tradingStrategy.TargetProfit);

        //decimal prixVenteCible2 = (tradingStrategy.TotalPurchaseCost + tradingStrategy.TargetProfit) / tradingStrategy.Quantity / (1 - tradingStrategy.FeePercentage);

        decimal commissionAchatBrute = tradingStrategy.CryptoPurchasePrice * tradingStrategy.Quantity * tradingStrategy.FeePercentage;
        decimal commissionAchat = commissionAchatBrute * (1 - tradingStrategy.Discount);
        decimal commissionVenteBrute = currentCurrencyPrice * tradingStrategy.Quantity * tradingStrategy.FeePercentage;
        decimal commissionVente = commissionVenteBrute * (1 - tradingStrategy.Discount);
        decimal prixVenteTotal = currentCurrencyPrice * tradingStrategy.Quantity;
        decimal prixAchatTotal = tradingStrategy.CryptoPurchasePrice * tradingStrategy.Quantity;
        decimal beneficeNet = (prixVenteTotal - commissionVente) - (prixAchatTotal + commissionAchat);
        decimal stopLossPrice = technicalIndicatorsCalculator.DetermineLossStrategy(tradingStrategy.CryptoPurchasePrice, volatility);

        if (currentCurrencyPrice < prixVenteCible)
            return (prixVenteCible, false);

        if (currentCurrencyPrice <= stopLossPrice)
        {
            logger.WriteLog("STOPP LOSS");
        }

        tradingStrategy.TotalBenefit += beneficeNet;
        logger.WriteLog($"===> [VENTE] {tradingStrategy.Quantity:F2} | " +
            $"currentCurrencyPrice: {currentCurrencyPrice:F2} | " +
            $"totalBenefit: {tradingStrategy.TotalBenefit:F2}");

        if (tradingStrategy.TestMode)
        {
            var orderResponse = await binanceClient.PlaceTestOrderAsync(symbol, tradingStrategy.Quantity, currentCurrencyPrice, "SELL");
            logger.WriteLog($"test : {JsonConvert.SerializeObject(orderResponse, Formatting.Indented)}");
        }
        else
        {
            var orderResponse = await binanceClient.PlaceOrderAsync(symbol, tradingStrategy.Quantity, currentCurrencyPrice, "SELL");
            logger.WriteLog($"real : {JsonConvert.SerializeObject(orderResponse, Formatting.Indented)}");
        }

        await WaitSellAsync(symbol);

        tradingStrategy.OpenPosition = false;

        //await _priceRetriever.HandleDiscountAsync(tradingStrategy);

        return (prixVenteCible, MaxBenefitHasBeenReached(tradingStrategy));
    }

    private bool MaxBenefitHasBeenReached(TradingStrategy tradingStrategy)
    {
        if (tradingStrategy.TotalBenefit >= tradingStrategy.LimitBenefit)
        {
            logger.WriteLog("BENEFICE LIMITE ->> exit program");
            return true;
        }

        return false;
    }

    public async Task WaitBuyAsync(string symbol)
    {
        var orders = await binanceClient.GetOpenOrdersAsync(symbol);

        while (true)
        {
            if (orders.Any((it) => it.Symbol == symbol && it.Side == "BUY"))
            {
                Console.WriteLine("En attente de la fin de l'achat");
                orders = await binanceClient.GetOpenOrdersAsync(symbol);
            }
            else
            {
                Console.WriteLine("Achat terminé");
                break;
            }
            await Task.Delay(300);
        }
    }

    public async Task WaitSellAsync(string symbol)
    {
        var orders = await binanceClient.GetOpenOrdersAsync(symbol);

        while (true)
        {
            if (orders.Any((it) => it.Symbol == symbol && it.Side == "SELL"))
            {
                Console.WriteLine("En attente de la fin de la vente");
                orders = await binanceClient.GetOpenOrdersAsync(symbol);
            }
            else
            {
                Console.WriteLine("Vente terminé");
                break;
            }
            await Task.Delay(300);
        }
    }
}