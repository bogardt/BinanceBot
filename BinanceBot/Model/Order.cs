﻿using Newtonsoft.Json;

namespace BinanceBot.Model
{
    public class Order
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        [JsonProperty("orderId")]
        public int OrderId { get; set; }
        [JsonProperty("side")]
        public string Side { get; set; }
    }
}
