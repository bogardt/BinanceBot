using BinanceBot.Abstraction;

namespace BinanceBot.Core
{
    public class TechnicalIndicatorsCalculator : ITechnicalIndicatorsCalculator
    {
        private readonly IPriceRetriever _priceRetriever;

        public TechnicalIndicatorsCalculator(IPriceRetriever priceRetriever)
        {
            _priceRetriever = priceRetriever;
        }

        public decimal CalculateMovingAverage(List<List<object>> klines, int periode)
        {
            var closingPrices = _priceRetriever.GetClosingPrices(klines);

            decimal somme = 0;

            foreach (var prix in closingPrices)
                somme += prix;

            return somme / periode;
        }

        public decimal CalculateRSI(List<List<object>> klines, int periode)
        {
            decimal gainMoyen = 0, perteMoyenne = 0;

            var closingPrices = _priceRetriever.GetClosingPrices(klines);

            for (int i = 1; i < closingPrices.Count(); i++)
            {
                decimal delta = closingPrices[i] - closingPrices[i - 1];

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

    }
}
