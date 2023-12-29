using Newtonsoft.Json;

namespace BinanceBot.Model
{
    public class Account
    {
        [JsonProperty("makerCommission")]
        public long MakerCommission { get; set; }

        [JsonProperty("takerCommission")]
        public long TakerCommission { get; set; }

        [JsonProperty("buyerCommission")]
        public long BuyerCommission { get; set; }

        [JsonProperty("sellerCommission")]
        public long SellerCommission { get; set; }

        [JsonProperty("commissionRates")]
        public CommissionRates CommissionRates { get; set; }

        [JsonProperty("canTrade")]
        public bool CanTrade { get; set; }

        [JsonProperty("canWithdraw")]
        public bool CanWithdraw { get; set; }

        [JsonProperty("canDeposit")]
        public bool CanDeposit { get; set; }

        [JsonProperty("brokered")]
        public bool Brokered { get; set; }

        [JsonProperty("requireSelfTradePrevention")]
        public bool RequireSelfTradePrevention { get; set; }

        [JsonProperty("preventSor")]
        public bool PreventSor { get; set; }

        [JsonProperty("updateTime")]
        public long UpdateTime { get; set; }

        [JsonProperty("accountType")]
        public string AccountType { get; set; }

        [JsonProperty("balances")]
        public Balance[] Balances { get; set; }

        [JsonProperty("permissions")]
        public string[] Permissions { get; set; }

        [JsonProperty("uid")]
        public long Uid { get; set; }
    }

}
