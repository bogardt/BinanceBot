namespace BinanceBot.Model
{
    public class StrategyCurrencyConfiguration
    {
        public decimal TargetProfit { get; set; }
        public decimal Quantity { get; set; }
        public int Period { get; set; }
        public string Interval { get; set; }
    }
}
