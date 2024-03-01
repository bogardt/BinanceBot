using TradingCalculation.Strategy;

namespace TradingCalculation;

public class TechnicalIndicatorsCalculator : ITechnicalIndicatorsCalculator
{
    private readonly StopLossStrategy _stopLossConfiguration = new();
    public bool IsTargetPriceAchievable(decimal targetPrice, List<decimal> closingPrices)
    {
        // Calculer la moyenne et l'écart type des prix de clôture
        decimal averagePrice = closingPrices.Average();
        decimal stdDeviation = CalculateStandardDeviation(closingPrices, averagePrice);

        // Estimer la probabilité d'atteindre le prix cible
        // Ici, on utilise une approche simple en considérant le prix cible par rapport à la moyenne ajustée par l'écart type
        // Cette méthode est rudimentaire et doit être ajustée en fonction de vos besoins spécifiques
        decimal upperBound = averagePrice + (2 * stdDeviation); // Utilise 2 écarts types comme marge

        return targetPrice <= upperBound;
    }

    public decimal CalculateStandardDeviation(List<decimal> values, decimal mean)
    {
        decimal sumOfSquaresOfDifferences = values.Sum(val => (val - mean) * (val - mean));
        decimal stdDev = (decimal)Math.Sqrt((double)(sumOfSquaresOfDifferences / values.Count));
        return stdDev;
    }
    public decimal CalculateProfit(decimal cryptoPurchasePrice, decimal cryptoSellingPrice, decimal quantity, decimal feePercentage, decimal discount)
    {
        decimal purchaseCommission = cryptoPurchasePrice * quantity * feePercentage;
        decimal discountOnPurchase = purchaseCommission * discount;
        decimal purchaseCommissionWithDiscount = purchaseCommission - discountOnPurchase;
        
        decimal effectivePurchaseCost = cryptoPurchasePrice * quantity + purchaseCommissionWithDiscount;

        decimal sellingCommission = cryptoSellingPrice * quantity * feePercentage;
        decimal discountOnSelling = sellingCommission * discount;
        decimal sellingCommissionWithDiscount = sellingCommission - discountOnSelling;

        decimal effectiveSellingRevenue = cryptoSellingPrice * quantity - sellingCommissionWithDiscount;

        decimal profit = effectiveSellingRevenue - effectivePurchaseCost;

        return profit;
    }

    public decimal CalculateMinimumSellingPrice(decimal cryptoPurchasePrice, decimal quantity, decimal feePercentage, decimal discount, decimal targetProfit)
    {
        decimal purchaseCommission = cryptoPurchasePrice * quantity * feePercentage;

        decimal discountOnPurchase = purchaseCommission * discount;
        decimal purchaseCommissionWithDiscount = purchaseCommission - discountOnPurchase;

        decimal effectiveCommissionRate = feePercentage * (1 - discount);

        decimal minimumSellingPrice = (cryptoPurchasePrice * quantity + purchaseCommissionWithDiscount + targetProfit) / (1 - effectiveCommissionRate);
        decimal minimumSellingPricePerItem = minimumSellingPrice / quantity;

        return minimumSellingPricePerItem;
    }

    public decimal CalculateMovingAverage(IEnumerable<decimal> closingPrices, int periode)
    {
        decimal somme = 0;

        foreach (var prix in closingPrices)
            somme += prix;

        return somme / periode;
    }

    public decimal CalculateRSI(IEnumerable<decimal> closingPrices, int periode)
    {
        decimal gainMoyen = 0, perteMoyenne = 0;

        for (int i = 1; i < closingPrices.Count(); i++)
        {
            decimal delta = closingPrices.ElementAt(i) - closingPrices.ElementAt(i - 1);

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

    public decimal CalculateVolatility(IEnumerable<decimal> closingPrices)
    {
        decimal moyenne = closingPrices.Average();
        decimal sumOfSquares = closingPrices.Sum(prix => (prix - moyenne) * (prix - moyenne));
        decimal ecartType = (decimal)Math.Sqrt((double)(sumOfSquares / (closingPrices.Count() - 1)));

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
