using Newtonsoft.Json;

namespace BinanceBot.Model
{
    public class CommissionRates
    {
        [JsonProperty("maker")]
        public string Maker { get; set; }

        [JsonProperty("taker")]
        public string Taker { get; set; }

        [JsonProperty("buyer")]
        public string Buyer { get; set; }

        [JsonProperty("seller")]
        public string Seller { get; set; }
    }
}