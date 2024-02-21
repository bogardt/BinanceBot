using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Model.Message;

public class Commission : BaseMessage
{
    [JsonProperty("standardCommission")]
    public CommissionRate? StandardCommission { get; set; }
    [JsonProperty("taxCommission")]
    public CommissionRate? TaxCommission { get; set; }
    [JsonProperty("discount")]
    public Discount? Discount { get; set; }
}
