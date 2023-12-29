namespace BinanceBot.Strategy
{
    public class StopLossStrategy
    {
        public decimal StopLossPercentage { get; set; } = 0.05m;
        public decimal VolatilityMultiplier { get; set; } = 2m;
        public decimal FloorStopLossPercentage { get; set; } = 0.012m;
        public decimal CeilingStopLossPercentage { get; set; } = 0.02m;
    }
}
