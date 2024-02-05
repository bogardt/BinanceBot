using Newtonsoft.Json;

namespace BinanceBot.Model;

public class Commission
{
    [JsonProperty("code")]
    public int? Code { get; set; }
    [JsonProperty("msg")]
    public string? Message { get; set; }
    [JsonProperty("standardCommission")]
    public CommissionRates? StandardCommission { get; set; }
    [JsonProperty("taxCommission")]
    public CommissionRates? TaxCommission { get; set; }
    [JsonProperty("discount")]
    public Discount? Discount { get; set; }
}
