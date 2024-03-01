using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Model;

public abstract class BaseMessage : IMessage
{
    [JsonProperty("code")]
    public int? Code { get; set; }
    [JsonProperty("msg")]
    public string? Message { get; set; }
}
