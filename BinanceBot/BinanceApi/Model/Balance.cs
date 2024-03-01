using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Model;

public class Balance : IMessage
{
    [JsonProperty("asset")]
    public string? Asset { get; set; }
    [JsonProperty("free")]
    public string? Free { get; set; }
}