using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Model;

public class Order : BaseMessage
{
    [JsonProperty("symbol")]
    public string? Symbol { get; set; }
    [JsonProperty("orderId")]
    public long? OrderId { get; set; }
    [JsonProperty("side")]
    public string? Side { get; set; }
}
