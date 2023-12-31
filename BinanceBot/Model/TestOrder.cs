﻿using Newtonsoft.Json;

namespace BinanceBot.Model
{
    public class TestOrder
    {
        [JsonProperty("code")]
        public int? Code { get; set; }
        [JsonProperty("msg")]
        public string? Message { get; set; }
        [JsonProperty("standardCommissionForOrder")]
        public CommissionRates StandardCommissionForOrder { get; set; }

        [JsonProperty("taxCommissionForOrder")]
        public CommissionRates TaxCommissionForOrder { get; set; }

        [JsonProperty("discount")]
        public Discount Discount { get; set; }
    }
}
