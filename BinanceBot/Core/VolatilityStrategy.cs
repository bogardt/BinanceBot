using BinanceBot.Abstraction;
using BinanceBot.Model;

namespace BinanceBot.Core
{
    public class VolatilityStrategy : IVolatilityStrategy
    {
        private readonly IPriceRetriever _priceRetriever;

        public VolatilityStrategy(IPriceRetriever priceRetriever)
        {
            _priceRetriever = priceRetriever;
        }
        public decimal CalculateVolatility(List<List<object>> klines)
        {
            var prixRecent = _priceRetriever.GetRecentPrices(klines);
            decimal moyenne = prixRecent.Average();
            decimal sommeDesCarres = prixRecent.Sum(prix => (prix - moyenne) * (prix - moyenne));
            decimal ecartType = (decimal)Math.Sqrt((double)(sommeDesCarres / 50));

            return ecartType;
        }

        public decimal DetermineLossStrategy(decimal volatility, TradingConfig tradingConfig)
        {
            decimal pourcentageStopLossCalcule = volatility * tradingConfig.VolatilityMultiplier;
            pourcentageStopLossCalcule = Math.Max(tradingConfig.FloorStopLossPercentage, pourcentageStopLossCalcule);
            pourcentageStopLossCalcule = Math.Min(tradingConfig.CeilingStopLossPercentage, pourcentageStopLossCalcule);
            tradingConfig.StopLossPercentage = (tradingConfig.StopLossPercentage + pourcentageStopLossCalcule) / 2;
            decimal prixStopLoss = tradingConfig.CryptoPurchasePrice * (1 - tradingConfig.StopLossPercentage);

            return prixStopLoss;
        }
    }
}
