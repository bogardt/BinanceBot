namespace BinanceBot.Strategy
{
    public class TradingStrategy
    {
        public decimal TargetProfit { get; set; } = 10m;
        public decimal Quantity { get; set; } = 100m;
        public int Period { get; set; } = 600;
        public string Interval { get; set; } = "1s";
        public string Symbol { get; set; } = "SOLUSDT";
        public decimal MaxRSI { get; set; } = 70;
        public decimal FeePercentage { get; set; } = 0.001m;
        public decimal Discount { get; set; } = 0m;
        public int LimitBenefit { get; set; } = 1000;
        public bool OpenPosition { get; set; } = false;
        public decimal TotalBenefit { get; set; } = 0;
        public decimal TotalPurchaseCost { get; set; } = 0;
        public decimal CryptoPurchasePrice { get; set; } = 0;
    }
}
