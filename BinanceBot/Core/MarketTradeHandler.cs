using Newtonsoft.Json;
using System.Globalization;
using BinanceBot.Model;
using BinanceBot.Abstraction;

namespace BinanceBot.Core
{
    public class MarketTradeHandler : IMarketTradeHandler
    {
        private readonly IBinanceClient _binanceClient;
        private readonly ILogger _logger;

        private static readonly string _symbol = "SOLUSDT";

        private static readonly Dictionary<string, StrategyCurrencyConfiguration> _dict = new()
        {
            { "SOLUSDT", new StrategyCurrencyConfiguration { TargetProfit = 10m, Quantity = 100m, Interval = "1m", Period = 20 } },
            { "ETHUSDT", new StrategyCurrencyConfiguration { TargetProfit = 10m, Quantity = 5m, Interval = "1s", Period = 300 } },
            { "ADAUSDT", new StrategyCurrencyConfiguration { TargetProfit = 1m, Quantity = 2000m, Interval = "1s", Period = 60 } }
        };

        private readonly TradingConfig _tradingConfig = new(_dict, _symbol);

        public MarketTradeHandler(IBinanceClient binanceClient,
            ILogger logger,
            TradingConfig? tradingConfig = null)
        {
            _binanceClient = binanceClient;
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

                    decimal mobileAverage = MobileAverageCalculation(klines, periode: _tradingConfig.Period);
                    decimal rsi = await RSICalculation(_symbol, _tradingConfig.Interval, periode: _tradingConfig.Period);
                    decimal volatility = VolatilityCalculation(klines);

                    decimal targetPriceFeesNotIncluded = ((currentCurrencyPrice * _tradingConfig.Quantity) + _tradingConfig.TargetProfit) / _tradingConfig.Quantity;
                    decimal targetPriceFeesIncluded = targetPriceFeesNotIncluded * (1 + _tradingConfig.FeePercentage);

                    if (!_tradingConfig.OpenPosition && rsi <= _tradingConfig.MaxRSI && currentCurrencyPrice < mobileAverage)
                    {
                        await Buy(currentCurrencyPrice, volatility);
                    }

                    if (_tradingConfig.OpenPosition)
                    {
                        var (targetPrice, endProgram) = await Sell(currentCurrencyPrice, volatility);
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

        private async Task Buy(decimal currentCurrencyPrice, decimal volatility)
        {
            _tradingConfig.CryptoPurchasePrice = currentCurrencyPrice;
            _tradingConfig.TotalPurchaseCost = _tradingConfig.Quantity * _tradingConfig.CryptoPurchasePrice;

            decimal feesAmount = _tradingConfig.TotalPurchaseCost * _tradingConfig.FeePercentage;
            _tradingConfig.TotalPurchaseCost *= (1 + _tradingConfig.FeePercentage);

            _logger.WriteLog($"===> [ACHAT] {_tradingConfig.Quantity} | " +
                $"feesAmount: {feesAmount} | " +
                $"cryptoPurchasePrice: {_tradingConfig.CryptoPurchasePrice:F2} | " +
                $"totalPurchaseCost: {_tradingConfig.TotalPurchaseCost:F2}");

            string responseAchat = await _binanceClient.PlaceOrderAsync(_symbol, _tradingConfig.Quantity, currentCurrencyPrice, "BUY");
            _logger.WriteLog($"{responseAchat}");

            await WaitBuyAsync();

            _tradingConfig.OpenPosition = true;
        }

        private async Task<(decimal, bool)> Sell(decimal currentCurrencyPrice, decimal volatility)
        {
            decimal prixVenteCible = (_tradingConfig.TotalPurchaseCost + _tradingConfig.TargetProfit) / _tradingConfig.Quantity / (1 - _tradingConfig.FeePercentage);

            decimal montantVenteBrut = currentCurrencyPrice * _tradingConfig.Quantity;
            decimal fraisVente = montantVenteBrut * _tradingConfig.FeePercentage;
            decimal montantVenteNet = (currentCurrencyPrice * _tradingConfig.Quantity) * (1 - _tradingConfig.FeePercentage);

            decimal beneficeBrut = montantVenteBrut - _tradingConfig.TotalPurchaseCost;
            decimal beneficeNet = montantVenteNet - _tradingConfig.TotalPurchaseCost;

            decimal stopLossPrice = PercentageLossStrategy(volatility);

            if (currentCurrencyPrice >= prixVenteCible)
            {
                if (currentCurrencyPrice <= stopLossPrice)
                {
                    _logger.WriteLog("STOPP LOSS");
                }

                _tradingConfig.TotalBenefit += beneficeNet;
                _logger.WriteLog($"===> [VENTE] {_tradingConfig.Quantity:F2} | " +
                    $"currentCurrencyPrice: {currentCurrencyPrice:F2} | " +
                    $"totalBenefit: {_tradingConfig.TotalBenefit:F2}");

                string responseVente = await _binanceClient.PlaceOrderAsync(_symbol, _tradingConfig.Quantity, currentCurrencyPrice, "SELL");
                _logger.WriteLog($"{responseVente}");

                await WaitSellAsync();

                if (_tradingConfig.TotalBenefit >= _tradingConfig.LimitBenefit)
                {
                    _logger.WriteLog("BENEFICE LIMITE ->> exit program");
                    return (prixVenteCible, true);
                }
                _tradingConfig.OpenPosition = false;
            }
            return (prixVenteCible, false);
        }

        private decimal PercentageLossStrategy(decimal volatility)
        {
            decimal pourcentageStopLossCalcule = volatility * _tradingConfig.VolatilityMultiplier;
            pourcentageStopLossCalcule = Math.Max(_tradingConfig.FloorStopLossPercentage, pourcentageStopLossCalcule);
            pourcentageStopLossCalcule = Math.Min(_tradingConfig.CeilingStopLossPercentage, pourcentageStopLossCalcule);
            _tradingConfig.StopLossPercentage = (_tradingConfig.StopLossPercentage + pourcentageStopLossCalcule) / 2;
            decimal prixStopLoss = _tradingConfig.CryptoPurchasePrice * (1 - _tradingConfig.StopLossPercentage);

            return prixStopLoss;
        }

        private static decimal MobileAverageCalculation(List<List<object>> klines, int periode)
        {
            var prixHistoriques = klines.Select((it) =>
            {
                if (!decimal.TryParse(it[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ret))
                    throw new InvalidCastException($"it cannot be converted to decimal for klines {JsonConvert.SerializeObject(it)}");
                return ret;
            }).ToList();

            decimal somme = 0;

            foreach (var prix in prixHistoriques)
                somme += prix;

            return somme / periode;
        }

        private async Task<decimal> RSICalculation(string symbol, string interval, int periode)
        {
            try
            {
                var klines = await _binanceClient.GetKLinesBySymbolAsync(symbol, interval, (periode + 1).ToString());
                decimal gainMoyen = 0, perteMoyenne = 0;

                var prixHistoriques = klines.Select((it) =>
                {
                    if (!decimal.TryParse(it[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ret))
                        throw new InvalidCastException($"it cannot be converted to decimal for klines {JsonConvert.SerializeObject(it)}");
                    return ret;
                }).ToList();

                for (int i = 1; i < prixHistoriques.Count(); i++)
                {
                    decimal delta = prixHistoriques[i] - prixHistoriques[i - 1];

                    if (delta > 0)
                        gainMoyen += delta;
                    else
                        perteMoyenne -= delta;
                }

                gainMoyen /= periode;
                perteMoyenne /= periode;

                decimal rs = perteMoyenne == 0 ? 0 : (gainMoyen / perteMoyenne);
                return 100 - (100 / (1 + rs));
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        private decimal VolatilityCalculation(List<List<object>> klines)
        {
            var prixRecent = RecuperePrixRecent(klines);
            decimal moyenne = prixRecent.Average();
            decimal sommeDesCarres = prixRecent.Sum(prix => (prix - moyenne) * (prix - moyenne));
            decimal ecartType = (decimal)Math.Sqrt((double)(sommeDesCarres / 50));

            return ecartType;
        }

        private List<decimal> RecuperePrixRecent(List<List<object>> klines)
        {
            try
            {
                var closingPrices = new List<decimal>();
                foreach (var kline in klines)
                {
                    if (!decimal.TryParse(kline[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ret))
                        throw new InvalidCastException($"it cannot be converted to decimal for klines {JsonConvert.SerializeObject(kline)}");
                    closingPrices.Add(ret); // 4 is the index for closing price
                }

                return closingPrices;
            }
            catch (HttpRequestException e)
            {
                _logger.WriteLog($"Erreur dans la requête HTTP: {e.Message}");
                return new List<decimal>();
            }
        }

        public async Task WaitBuyAsync()
        {
            var orders = await _binanceClient.GetOpenOrdersAsync(_symbol);

            while (true)
            {
                if (orders.Any((it) => it.Symbol == _symbol && it.Side == "BUY"))
                {
                    Console.WriteLine("En attente de la fin de l'achat");
                    orders = await _binanceClient.GetOpenOrdersAsync(_symbol);
                }
                else
                {
                    Console.WriteLine("Achat terminé");
                    break;
                }
                await Task.Delay(300);
            }
        }

        public async Task WaitSellAsync()
        {
            var orders = await _binanceClient.GetOpenOrdersAsync(_symbol);

            while (true)
            {
                if (orders.Any((it) => it.Symbol == _symbol && it.Side == "SELL"))
                {
                    Console.WriteLine("En attente de la fin de la vente");
                    orders = await _binanceClient.GetOpenOrdersAsync(_symbol);
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
