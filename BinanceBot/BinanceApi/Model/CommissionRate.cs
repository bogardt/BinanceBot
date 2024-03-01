using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Model;

public class CommissionRate : IMessage
{
    [JsonProperty("maker")]
    public string? Maker { get; set; }
    [JsonProperty("taker")]
    public string? Taker { get; set; }
    [JsonProperty("buyer")]
    public string? Buyer { get; set; }
    [JsonProperty("seller")]
    public string? Seller { get; set; }
}