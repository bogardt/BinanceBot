namespace BinanceBot.Model
{
    public class TradingConfig
    {
        public decimal FeePercentage { get; set; }
        public decimal TargetProfit { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalBenefit { get; set; } = 0;
        public decimal MaxRSI { get; set; } = 70;
        public decimal TotalPurchaseCost { get; set; } = 0;
        public decimal CryptoPurchasePrice { get; set; } = 0;
        public int LimitBenefit { get; set; } = 500;
        public int Period { get; set; }
        public string Interval { get; set; }
        public bool OpenPosition { get; set; } = false;
        public decimal StopLossPercentage { get; set; }
        public decimal VolatilityMultiplier { get; set; }
        public decimal FloorStopLossPercentage { get; set; }
        public decimal CeilingStopLossPercentage { get; set; }

        public TradingConfig(Dictionary<string, StrategyCurrencyConfiguration> dict, string symbol)
        {
            FeePercentage = 0.001m;
            TargetProfit = dict[symbol].ProfitCible;
            Quantity = dict[symbol].QuantiteFixeCryptoAcheter;
            Period = dict[symbol].Periode;
            Interval = dict[symbol].Interval;
            StopLossPercentage = 0.05m;
            VolatilityMultiplier = 2;
            FloorStopLossPercentage = 0.012m;
            CeilingStopLossPercentage = 0.02m;
        }
    }
}
