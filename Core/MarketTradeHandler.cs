using Newtonsoft.Json;
using System.Globalization;
using BinanceBot.BinanceApi;
using BinanceBot.Logger;
using BinanceBot.Model;

namespace BinanceBot.Core
{
    public class MarketTradeHandler : IMarketTradeHandler
    {
        private readonly IBinanceClient _binanceClient;
        private readonly ILogger _logger;

        private static readonly string _symbol = "SOLUSDT";

        private static readonly Dictionary<string, CurrencyConfiguration> _dict = new Dictionary<string, CurrencyConfiguration>
        {
            { "SOLUSDT", new CurrencyConfiguration { ProfitCible = 50m, QuantiteFixeCryptoAcheter = 100m, Interval = "1s", Periode = 900 } },
            { "ETHUSDT", new CurrencyConfiguration { ProfitCible = 10m, QuantiteFixeCryptoAcheter = 5m, Interval = "1s", Periode = 900 } },
            { "ADAUSDT", new CurrencyConfiguration { ProfitCible = 1m, QuantiteFixeCryptoAcheter = 2000m, Interval = "1s", Periode = 60 } }
        };

        private readonly TradingConfig _tradingConfig = new TradingConfig(_dict, _symbol);

        public MarketTradeHandler(IBinanceClient binanceClient,
            ILogger logger)
        {
            _binanceClient = binanceClient;
            _logger = logger;
        }

        public async Task TradeLimit()
        {
            while (true)
            {
                try
                {
                    var startLoop = DateTime.UtcNow;

                    var getPriceTask = _binanceClient.GetPriceBySymbolAsync(_symbol);
                    var getKlinesTask = _binanceClient.GetKLinesBySymbolAsync(_symbol, _tradingConfig.Interval, _tradingConfig.Period.ToString());

                    await Task.WhenAll(getPriceTask, getKlinesTask);

                    var currency = await getPriceTask;
                    var klines = await getKlinesTask;

                    decimal currentCurrencyPrice = currency.Price;
                    decimal moyenneMobile, rsi;

                    var moyenneMobileTask = MobileAverageCalculation(klines, periode: _tradingConfig.Period);
                    var rsiTask = RSICalculation(_symbol, _tradingConfig.Interval, periode: _tradingConfig.Period);
                    decimal volatility = VolatilityCalculation(klines);

                    await Task.WhenAll(moyenneMobileTask, rsiTask);

                    moyenneMobile = await moyenneMobileTask;
                    rsi = await rsiTask;

                    decimal prixVenteCibleParCrypto = ((currentCurrencyPrice * _tradingConfig.FixedQuantityCryptoToBuy) + _tradingConfig.TargetProfit) / _tradingConfig.FixedQuantityCryptoToBuy;
                    decimal prixVenteCibleParCryptoAvecFrais = prixVenteCibleParCrypto * (1 + _tradingConfig.FeePercentage);

                    if (!_tradingConfig.OpenPosition && rsi <= _tradingConfig.MaxRSI && prixVenteCibleParCrypto < moyenneMobile)
                    {
                        await Buy(currentCurrencyPrice, volatility);
                    }

                    if (_tradingConfig.OpenPosition)
                    {
                        decimal prixVenteCible = await Sell(currentCurrencyPrice, volatility);

                        decimal beneficePossible = (_tradingConfig.TotalPurchaseCost + _tradingConfig.TargetProfit) - _tradingConfig.TotalPurchaseCost;

                        _logger.WriteLog($"timeElapsed {DateTime.UtcNow - startLoop} | " +
                            $"diffMarge : {(prixVenteCible - _tradingConfig.CryptoPurchasePrice):F2} | " +
                            $"{_symbol} : {currentCurrencyPrice:F2} | " +
                            $"prixVenteCible : {prixVenteCible:F2} | " +
                            $"moyenne mobile : {moyenneMobile:F2} | " +
                            $"rsi : {rsi:F2} | " +
                            $"beneficePossible: {beneficePossible:F2} | " +
                            $"Bénéfice total: {_tradingConfig.TotalBenefit:F2} | " +
                            $"Quantity: {_tradingConfig.FixedQuantityCryptoToBuy} | " +
                            $"Vol : {volatility:F2}");
                    }
                    else
                    {
                        _logger.WriteLog($"timeElapsed : {DateTime.UtcNow - startLoop} | " +
                            $"{_symbol} : {currentCurrencyPrice:F2} | " +
                            $"WithoutFee : {prixVenteCibleParCrypto:F2} | " +
                            $"WithFee : {prixVenteCibleParCryptoAvecFrais:F2} | " +
                            $"moyenne mobile : {moyenneMobile:F2} | rsi : {rsi:F2} | " +
                            $"Bénéfice total: {_tradingConfig.TotalBenefit:F2} | " +
                            $"Quantity :  {_tradingConfig.FixedQuantityCryptoToBuy} | " +
                            $"Vol : {volatility:F2}");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur: {ex.Message}", ex);
                }
            }
        }

        private async Task Buy(decimal currentCurrencyPrice, decimal volatility)
        {
            _tradingConfig.CryptoPurchasePrice = currentCurrencyPrice;
            _tradingConfig.TotalPurchaseCost = _tradingConfig.FixedQuantityCryptoToBuy * _tradingConfig.CryptoPurchasePrice;

            decimal feesAmount = _tradingConfig.TotalPurchaseCost * _tradingConfig.FeePercentage;
            _tradingConfig.TotalPurchaseCost *= (1 + _tradingConfig.FeePercentage);

            _logger.WriteLog($"===> [ACHAT] {_tradingConfig.FixedQuantityCryptoToBuy} | " +
                $"feesAmount : {feesAmount} | " +
                $"prixAchatCrypto: {_tradingConfig.CryptoPurchasePrice:F2} | " +
                $"coutTotalAchat : {_tradingConfig.TotalPurchaseCost:F2}");

            string responseAchat = await _binanceClient.PlaceOrder(_symbol, _tradingConfig.FixedQuantityCryptoToBuy, currentCurrencyPrice, "BUY");
            _logger.WriteLog($"{responseAchat}");

            await WaitBuy();

            _tradingConfig.OpenPosition = true;
        }

        private async Task<decimal> Sell(decimal currentCurrencyPrice, decimal volatility)
        {
            decimal prixVenteCible = (_tradingConfig.TotalPurchaseCost + _tradingConfig.TargetProfit) / _tradingConfig.FixedQuantityCryptoToBuy / (1 - _tradingConfig.FeePercentage);

            decimal montantVenteBrut = currentCurrencyPrice * _tradingConfig.FixedQuantityCryptoToBuy;
            decimal fraisVente = montantVenteBrut * _tradingConfig.FeePercentage;
            decimal montantVenteNet = (currentCurrencyPrice * _tradingConfig.FixedQuantityCryptoToBuy) * (1 - _tradingConfig.FeePercentage);

            decimal beneficeBrut = montantVenteBrut - _tradingConfig.TotalPurchaseCost;
            decimal beneficeNet = montantVenteNet - _tradingConfig.TotalPurchaseCost;

            decimal stopLossPrice = PercentageLossStrategy(volatility);

            if (currentCurrencyPrice >= prixVenteCible)
            {
                if (currentCurrencyPrice <= stopLossPrice)
                {
                    _logger.WriteLog("STOPP LOSS");
                }

                // Mise à jour du bénéfice total
                _tradingConfig.TotalBenefit += beneficeNet;
                _logger.WriteLog($"===> [VENTE] {_tradingConfig.FixedQuantityCryptoToBuy:F2} | " +
                    $"prixActuel {currentCurrencyPrice:F2} | " +
                    $"Bénéfice total: {_tradingConfig.TotalBenefit:F2}");

                string responseVente = await _binanceClient.PlaceOrder(_symbol, _tradingConfig.FixedQuantityCryptoToBuy, currentCurrencyPrice, "SELL");
                _logger.WriteLog($"{responseVente}");

                await WaitSell();

                if (_tradingConfig.TotalBenefit >= _tradingConfig.LimitBenefit)
                {
                    _logger.WriteLog("BENEFICE LIMITE ->> exit program");
                    Console.ReadLine();
                }
                _tradingConfig.OpenPosition = false;
            }
            return prixVenteCible;
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

        private async Task<decimal> MobileAverageCalculation(List<List<object>> klines, int periode)
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

        public async Task WaitBuy()
        {
            var orders = await _binanceClient.GetOpenOrdersAsync(_symbol);

            while (true)
            {
                if (orders.Any((it) => it.symbol == _symbol && it.side == "BUY"))
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

        public async Task WaitSell()
        {
            var orders = await _binanceClient.GetOpenOrdersAsync(_symbol);

            while (true)
            {
                if (orders.Any((it) => it.symbol == _symbol && it.side == "SELL"))
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
