using Newtonsoft.Json;

namespace BinanceBot.Model;

public class Currency
{
    [JsonProperty("code")]
    public int? Code { get; set; }
    [JsonProperty("msg")]
    public string? Message { get; set; }
    [JsonProperty("symbol")]
    public string? Symbol { get; set; }
    [JsonProperty("price")]
    public decimal Price { get; set; }
}
