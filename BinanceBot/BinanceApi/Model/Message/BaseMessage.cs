using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Model.Message;

public abstract class BaseMessage
{
    [JsonProperty("code")]
    public int? Code { get; set; }
    [JsonProperty("msg")]
    public string? Message { get; set; }
}
