using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Model;

public class Currency : BaseMessage
{
    [JsonProperty("symbol")]
    public string? Symbol { get; set; }
    [JsonProperty("price")]
    public decimal? Price { get; set; }
}
