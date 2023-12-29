using BinanceBot.Abstraction;
using BinanceBot.Strategy;
using Newtonsoft.Json;

namespace BinanceBot.Core
{
    public class TradeAction : ITradeAction
    {

        private readonly IBinanceClient _binanceClient;
        private readonly IVolatilityStrategy _volatilityStrategy;
        private readonly IPriceRetriever _priceRetriever;
        private readonly ILogger _logger;
        public TradeAction(IBinanceClient binanceClient, 
            IVolatilityStrategy volatilityStrategy, 
            IPriceRetriever priceRetriever,
            ILogger logger)
        {
            _binanceClient = binanceClient;
            _volatilityStrategy = volatilityStrategy;
            _priceRetriever = priceRetriever;
            _logger = logger;
        }

        public async Task Buy(TradingStrategy tradingConfig, decimal currentCurrencyPrice, decimal volatility, string symbol)
        {
            tradingConfig.CryptoPurchasePrice = currentCurrencyPrice;
            tradingConfig.TotalPurchaseCost = tradingConfig.Quantity * tradingConfig.CryptoPurchasePrice;

            decimal feesAmount = tradingConfig.TotalPurchaseCost * tradingConfig.FeePercentage;
            tradingConfig.TotalPurchaseCost *= (1 + tradingConfig.FeePercentage);

            _logger.WriteLog($"===> [ACHAT] {tradingConfig.Quantity} | " +
                $"feesAmount: {feesAmount} | " +
                $"cryptoPurchasePrice: {tradingConfig.CryptoPurchasePrice:F2} | " +
                $"totalPurchaseCost: {tradingConfig.TotalPurchaseCost:F2}");

            var response = await _binanceClient.PlaceTestOrderAsync(symbol, tradingConfig.Quantity, currentCurrencyPrice, "BUY");
            _logger.WriteLog($"{JsonConvert.SerializeObject(response, Formatting.Indented)}");

            await WaitBuyAsync(symbol);

            tradingConfig.OpenPosition = true;
        }

        public async Task<(decimal, bool)> Sell(TradingStrategy tradingConfig,
            decimal currentCurrencyPrice,
            decimal volatility,
            string symbol)
        {
            decimal prixVenteCible = _priceRetriever.CalculateMinimumSellingPrice(
                tradingConfig.CryptoPurchasePrice,
                tradingConfig.Quantity,
                tradingConfig.FeePercentage,
                tradingConfig.Discount,
                tradingConfig.TargetProfit);
            //decimal prixVenteCible2 = (tradingConfig.TotalPurchaseCost + tradingConfig.TargetProfit) / tradingConfig.Quantity / (1 - tradingConfig.FeePercentage);
            
            decimal commissionAchatBrute = tradingConfig.CryptoPurchasePrice * tradingConfig.Quantity * tradingConfig.FeePercentage;
            decimal commissionAchat = commissionAchatBrute * (1 - tradingConfig.Discount);
            decimal commissionVenteBrute = currentCurrencyPrice * tradingConfig.Quantity * tradingConfig.FeePercentage;
            decimal commissionVente = commissionVenteBrute * (1 - tradingConfig.Discount);
            decimal prixVenteTotal = currentCurrencyPrice * tradingConfig.Quantity;
            decimal prixAchatTotal = tradingConfig.CryptoPurchasePrice * tradingConfig.Quantity;
            decimal beneficeNet = (prixVenteTotal - commissionVente) - (prixAchatTotal + commissionAchat);
            decimal stopLossPrice = _volatilityStrategy.DetermineLossStrategy(tradingConfig.CryptoPurchasePrice, volatility);

            if (currentCurrencyPrice >= prixVenteCible)
            {
                if (currentCurrencyPrice <= stopLossPrice)
                {
                    _logger.WriteLog("STOPP LOSS");
                }

                tradingConfig.TotalBenefit += beneficeNet;
                _logger.WriteLog($"===> [VENTE] {tradingConfig.Quantity:F2} | " +
                    $"currentCurrencyPrice: {currentCurrencyPrice:F2} | " +
                    $"totalBenefit: {tradingConfig.TotalBenefit:F2}");

                var response = await _binanceClient.PlaceTestOrderAsync(symbol, tradingConfig.Quantity, currentCurrencyPrice, "SELL");
                _logger.WriteLog($"{JsonConvert.SerializeObject(response, Formatting.Indented)}");

                await WaitSellAsync(symbol);

                tradingConfig.OpenPosition = false;

                if (tradingConfig.TotalBenefit >= tradingConfig.LimitBenefit)
                {
                    _logger.WriteLog("BENEFICE LIMITE ->> exit program");
                    return (prixVenteCible, true);
                }
            }
            return (prixVenteCible, false);
        }

        public async Task WaitBuyAsync(string symbol)
        {
            var orders = await _binanceClient.GetOpenOrdersAsync(symbol);

            while (true)
            {
                if (orders.Any((it) => it.Symbol == symbol && it.Side == "BUY"))
                {
                    Console.WriteLine("En attente de la fin de l'achat");
                    orders = await _binanceClient.GetOpenOrdersAsync(symbol);
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
            var orders = await _binanceClient.GetOpenOrdersAsync(symbol);

            while (true)
            {
                if (orders.Any((it) => it.Symbol == symbol && it.Side == "SELL"))
                {
                    Console.WriteLine("En attente de la fin de la vente");
                    orders = await _binanceClient.GetOpenOrdersAsync(symbol);
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
}