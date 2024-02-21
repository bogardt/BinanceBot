using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Model;

public class Balance
{
    [JsonProperty("asset")]
    public string? Asset { get; set; }
    [JsonProperty("free")]
    public string? Free { get; set; }
}