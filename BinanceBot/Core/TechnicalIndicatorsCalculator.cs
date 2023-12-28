using BinanceBot.Abstraction;
using Newtonsoft.Json;
using System.Globalization;

namespace BinanceBot.Core
{
    public class TechnicalIndicatorsCalculator : ITechnicalIndicatorsCalculator
    {
        private readonly IBinanceClient _binanceClient;

        public TechnicalIndicatorsCalculator(IBinanceClient binanceClient)
        {
            _binanceClient = binanceClient;
        }
        public decimal CalculateMovingAverage(List<List<object>> klines, int periode)
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

        public decimal CalculateRSI(List<List<object>> klines, int periode)
        {
            try
            {
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

    }
}
