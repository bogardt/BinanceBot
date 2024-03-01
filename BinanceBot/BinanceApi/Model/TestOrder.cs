using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Model;

public class TestOrder : BaseMessage
{
    [JsonProperty("standardCommissionForOrder")]
    public CommissionRate? StandardCommissionForOrder { get; set; }
    [JsonProperty("taxCommissionForOrder")]
    public CommissionRate? TaxCommissionForOrder { get; set; }
    [JsonProperty("discount")]
    public Discount? Discount { get; set; }
}
