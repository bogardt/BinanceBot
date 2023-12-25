using Newtonsoft.Json;

namespace BinanceBot.Model
{
    public class Currency
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        [JsonProperty("price")]
        public decimal Price { get; set; }
    }
}
