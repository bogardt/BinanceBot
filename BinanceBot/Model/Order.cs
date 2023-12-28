using Newtonsoft.Json;

namespace BinanceBot.Model
{
    public class Order
    {
        [JsonProperty("symbol")]
        public required string Symbol { get; set; }

        [JsonProperty("orderId")]
        public long OrderId { get; set; }

        [JsonProperty("orderListId")]
        public int OrderListId { get; set; }

        [JsonProperty("clientOrderId")]
        public string ClientOrderId { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("origQty")]
        public string OrigQty { get; set; }

        [JsonProperty("executedQty")]
        public string ExecutedQty { get; set; }

        [JsonProperty("cummulativeQuoteQty")]
        public string CummulativeQuoteQty { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("timeInForce")]
        public string TimeInForce { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("stopPrice")]
        public string StopPrice { get; set; }

        [JsonProperty("icebergQty")]
        public string IcebergQty { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("updateTime")]
        public long UpdateTime { get; set; }

        [JsonProperty("isWorking")]
        public bool IsWorking { get; set; }

        [JsonProperty("origQuoteOrderQty")]
        public string OrigQuoteOrderQty { get; set; }

        [JsonProperty("selfTradePreventionMode")]
        public string SelfTradePreventionMode { get; set; }
    }
}
