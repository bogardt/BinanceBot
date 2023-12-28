using Newtonsoft.Json;

namespace BinanceBot.Model
{
    public class Order
    {
        [JsonProperty("symbol")]
        public required string Symbol { get; set; }

        [JsonProperty("orderId")]
        public long OrderId { get; set; }
        [JsonProperty("price")]
        public string Price { get; set; }
        [JsonProperty("side")]
        public string Side { get; set; }
        [JsonProperty("time")]
        public long Time { get; set; }
    }
}
