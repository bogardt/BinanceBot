using Newtonsoft.Json;

namespace BinanceBot.Model
{
    public class Order
    {
        [JsonProperty("symbol")]
        public required string Symbol { get; set; }
        [JsonProperty("side")]
        public string Side { get; set; }
    }
}
