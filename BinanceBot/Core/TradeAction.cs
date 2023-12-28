using BinanceBot.Abstraction;

namespace BinanceBot.Core
{
    public class TradeAction : ITradeAction
    {

        private readonly IBinanceClient _binanceClient;
        private readonly IVolatilityStrategy _volatilityStrategy;
        private readonly ILogger _logger;
        public TradeAction(IBinanceClient binanceClient, 
            IVolatilityStrategy volatilityStrategy, 
            ILogger logger)
        {
            _binanceClient = binanceClient;
            _volatilityStrategy = volatilityStrategy;
            _logger = logger;
        }

        public async Task Buy(TradingConfig tradingConfig, decimal currentCurrencyPrice, decimal volatility, string symbol)
        {
            tradingConfig.CryptoPurchasePrice = currentCurrencyPrice;
            tradingConfig.TotalPurchaseCost = tradingConfig.Quantity * tradingConfig.CryptoPurchasePrice;

            decimal feesAmount = tradingConfig.TotalPurchaseCost * tradingConfig.FeePercentage;
            tradingConfig.TotalPurchaseCost *= (1 + tradingConfig.FeePercentage);

            _logger.WriteLog($"===> [ACHAT] {tradingConfig.Quantity} | " +
                $"feesAmount: {feesAmount} | " +
                $"cryptoPurchasePrice: {tradingConfig.CryptoPurchasePrice:F2} | " +
                $"totalPurchaseCost: {tradingConfig.TotalPurchaseCost:F2}");

            string responseAchat = await _binanceClient.PlaceOrderAsync(symbol, tradingConfig.Quantity, currentCurrencyPrice, "BUY");
            _logger.WriteLog($"{responseAchat}");

            await WaitBuyAsync(symbol);

            tradingConfig.OpenPosition = true;
        }

        public async Task<(decimal, bool)> Sell(TradingConfig tradingConfig, decimal currentCurrencyPrice, decimal volatility, string symbol)
        {
            decimal prixVenteCible = (tradingConfig.TotalPurchaseCost + tradingConfig.TargetProfit) / tradingConfig.Quantity / (1 - tradingConfig.FeePercentage);

            decimal montantVenteBrut = currentCurrencyPrice * tradingConfig.Quantity;
            decimal fraisVente = montantVenteBrut * tradingConfig.FeePercentage;
            decimal montantVenteNet = (currentCurrencyPrice * tradingConfig.Quantity) * (1 - tradingConfig.FeePercentage);

            decimal beneficeBrut = montantVenteBrut - tradingConfig.TotalPurchaseCost;
            decimal beneficeNet = montantVenteNet - tradingConfig.TotalPurchaseCost;

            decimal stopLossPrice = _volatilityStrategy.DetermineLossStrategy(volatility, tradingConfig);

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

                string responseVente = await _binanceClient.PlaceOrderAsync(symbol, tradingConfig.Quantity, currentCurrencyPrice, "SELL");
                _logger.WriteLog($"{responseVente}");

                await WaitSellAsync(symbol);

                if (tradingConfig.TotalBenefit >= tradingConfig.LimitBenefit)
                {
                    _logger.WriteLog("BENEFICE LIMITE ->> exit program");
                    return (prixVenteCible, true);
                }
                tradingConfig.OpenPosition = false;
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