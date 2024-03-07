namespace TradingCalculation.Strategy;

public class TradingStrategy
{
    public decimal TargetProfit { get; set; } = 100m;
    public decimal Quantity { get; set; } = 150m;
    public decimal MaxRSI { get; set; } = 70;
    public decimal FeePercentage { get; set; } = 0.001m;
    public decimal Discount { get; set; } = 0m;
    public decimal TotalBenefit { get; set; } = 0;
    public decimal TotalPurchaseCost { get; set; } = 0;
    public decimal CryptoPurchasePrice { get; set; } = 0;
    public int Period { get; set; } = 60;
    public string Interval { get; set; } = "1m";
    public string Symbol { get; set; } = "SOLUSDT";
    public int LimitBenefit { get; set; } = 3000;
    public bool OpenPosition { get; set; } = false;
    public bool TestMode { get; set; } = true;
}
