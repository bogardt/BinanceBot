using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Model;

public class BinanceKline
{
    [JsonProperty(Order = 0)]
    public long OpenTime { get; set; }

    [JsonProperty(Order = 1)]
    public decimal Open { get; set; }

    [JsonProperty(Order = 2)]
    public decimal High { get; set; }

    [JsonProperty(Order = 3)]
    public decimal Low { get; set; }

    [JsonProperty(Order = 4)]
    public decimal Close { get; set; }

    [JsonProperty(Order = 5)]
    public decimal Volume { get; set; }

    [JsonProperty(Order = 6)]
    public long CloseTime { get; set; }

    [JsonProperty(Order = 7)]
    public decimal QuoteAssetVolume { get; set; }

    [JsonProperty(Order = 8)]
    public int NumberOfTrades { get; set; }

    [JsonProperty(Order = 9)]
    public decimal TakerBuyBaseAssetVolume { get; set; }

    [JsonProperty(Order = 10)]
    public decimal TakerBuyQuoteAssetVolume { get; set; }

}
