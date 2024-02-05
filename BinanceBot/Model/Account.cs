using Newtonsoft.Json;

namespace BinanceBot.Model;

public class Account
{
    [JsonProperty("code")]
    public int? Code {  get; set; }
    [JsonProperty("msg")]
    public string? Message { get; set; }
    [JsonProperty("balances")]
    public Balance[] Balances { get; set; }
}
