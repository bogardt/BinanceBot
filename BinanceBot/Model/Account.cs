using Newtonsoft.Json;

namespace BinanceBot.Model
{
    public class Account
    {
        [JsonProperty("balances")]
        public Balance[] Balances { get; set; }
    }
}
