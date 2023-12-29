using Newtonsoft.Json;

namespace BinanceBot.Model
{
    public class Discount
    {
        [JsonProperty("enabledForAccount")]
        public bool EnabledForAccount { get; set; }

        [JsonProperty("enabledForSymbol")]
        public bool EnabledForSymbol { get; set; }

        [JsonProperty("discountAsset")]
        public string DiscountAsset { get; set; }

        [JsonProperty("discount")]
        public string DiscountValue { get; set; }
    }
}