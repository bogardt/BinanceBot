using BinanceBot.Abstraction;
using BinanceBot.Strategy;

namespace BinanceBot.Core
{
    public class VolatilityStrategy : IVolatilityStrategy
    {
        private readonly IPriceRetriever _priceRetriever;
        private readonly StopLossStrategy _stopLossConfiguration = new();

        public VolatilityStrategy(IPriceRetriever priceRetriever)
        {
            _priceRetriever = priceRetriever;
        }
        public decimal CalculateVolatility(List<List<object>> klines)
        {
            var prixRecent = _priceRetriever.GetClosingPrices(klines);
            decimal moyenne = prixRecent.Average();
            decimal sommeDesCarres = prixRecent.Sum(prix => (prix - moyenne) * (prix - moyenne));
            decimal ecartType = (decimal)Math.Sqrt((double)(sommeDesCarres / 50));

            return ecartType;
        }

        public decimal DetermineLossStrategy(decimal cryptoPurchasePrice, decimal volatility)
        {
            decimal pourcentageStopLossCalcule = volatility * _stopLossConfiguration.VolatilityMultiplier;
            pourcentageStopLossCalcule = Math.Max(_stopLossConfiguration.FloorStopLossPercentage, pourcentageStopLossCalcule);
            pourcentageStopLossCalcule = Math.Min(_stopLossConfiguration.CeilingStopLossPercentage, pourcentageStopLossCalcule);
            _stopLossConfiguration.StopLossPercentage = (_stopLossConfiguration.StopLossPercentage + pourcentageStopLossCalcule) / 2;
            decimal prixStopLoss = cryptoPurchasePrice * (1 - _stopLossConfiguration.StopLossPercentage);

            return prixStopLoss;
        }
    }
}
