namespace BinanceBot.Model
{
    public class StrategyCurrencyConfiguration
    {
        public decimal ProfitCible { get; set; }
        public decimal QuantiteFixeCryptoAcheter { get; set; }
        public int Periode { get; set; }
        public string Interval { get; set; }
    }
}
