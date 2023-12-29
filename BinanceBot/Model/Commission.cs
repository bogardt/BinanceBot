using Newtonsoft.Json;

namespace BinanceBot.Model
{
    public class Commission
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("standardCommission")]
        public CommissionRates StandardCommission { get; set; }

        [JsonProperty("taxCommission")]
        public CommissionRates TaxCommission { get; set; }

        [JsonProperty("discount")]
        public Discount Discount { get; set; }
    }
}
