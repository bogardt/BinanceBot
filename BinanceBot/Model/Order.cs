using Newtonsoft.Json;

namespace BinanceBot.Model;

public class Order
{
    [JsonProperty("code")]
    public int? Code { get; set; }
    [JsonProperty("msg")]
    public string? Message { get; set; }
    [JsonProperty("symbol")]
    public string Symbol { get; set; }
    [JsonProperty("orderId")]
    public int OrderId { get; set; }
    [JsonProperty("side")]
    public string Side { get; set; }
}
