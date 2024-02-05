using BinanceBot.Abstraction;
using BinanceBot.Strategy;

namespace BinanceBot.Core;

public class TechnicalIndicatorsCalculator : ITechnicalIndicatorsCalculator
{
    private readonly StopLossStrategy _stopLossConfiguration = new();

    public decimal CalculateMovingAverage(List<decimal> closingPrices, int periode)
    {
        decimal somme = 0;

        foreach (var prix in closingPrices)
            somme += prix;

        return somme / periode;
    }

    public decimal CalculateRSI(List<decimal> closingPrices, int periode)
    {
        decimal gainMoyen = 0, perteMoyenne = 0;

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

    public decimal CalculateVolatility(List<decimal> closingPrices)
    {
        decimal moyenne = closingPrices.Average();
        decimal sumOfSquares = closingPrices.Sum(prix => (prix - moyenne) * (prix - moyenne));
        decimal ecartType = (decimal)Math.Sqrt((double)(sumOfSquares / (closingPrices.Count - 1)));

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
