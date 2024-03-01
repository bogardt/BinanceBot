using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Model;

public class Account : BaseMessage
{
    [JsonProperty("balances")]
    public Balance[]? Balances { get; set; }
}
