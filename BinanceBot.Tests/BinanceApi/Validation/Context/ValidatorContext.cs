using BinanceBot.BinanceApi.Model;

namespace BinanceBot.Tests.BinanceApi.Validation.Context
{
    public static class ValidatorContext
    {
        public static T ErrorMessage<T>() where T : BaseMessage, new()
        {
            T instance = new()
            {
                Code = 100,
                Message = "KO"
            };

            return instance;
        }
        public static readonly Account accountValid = new()
        {
            Balances = [Component.balanceValid!]
        };

        public static readonly Account accountBalanceEmpty = new()
        {
            Balances = [new()]
        };

        public static readonly Commission commissionValid = new()
        {
            Discount = Component.discountValid,
            StandardCommission = Component.commissionRateValid,
            TaxCommission = Component.commissionRateValid
        };

        public static readonly Commission commissionValidWithStdValidAndTaxNull = new()
        {
            Discount = Component.discountValid,
            StandardCommission = Component.commissionRateValid,
            TaxCommission = Component.commissionRateNullOrEmpty
        };


        public static readonly Commission commissionUnvalidWithStdNullAndTaxValid = new()
        {
            Discount = Component.discountUnvalid,
            StandardCommission = Component.commissionRateNullOrEmpty,
            TaxCommission = Component.commissionRateValid
        };

        public static class Component
        {
            public static readonly Balance balanceEmpty = new();

            public static readonly Balance balanceValid = new()
            {
                Asset = "0.1",
                Free = "0.1"
            };

            public static readonly CommissionRate commissionRateValid = new()
            {
                Buyer = "0.001",
                Maker = "0.001",
                Seller = "0.001",
                Taker = "0.001",
            };

            public static readonly CommissionRate commissionRateUnvalidCharacters = new()
            {
                Buyer = "0.001",
                Maker = "test", // todo
                Seller = "0.001",
                Taker = "0.001",
            };

            public static readonly CommissionRate commissionRateNullOrEmpty = new()
            {
                Buyer = null,
                Maker = null,
                Seller = null,
                Taker = null
            };

            public static readonly Discount discountValid = new()
            {
                DiscountAsset = "BNB",
                DiscountValue = "0.75000000",
                EnabledForAccount = true,
                EnabledForSymbol = true,
            };

            public static readonly Discount discountUnvalid = new()
            {
                DiscountAsset = "",
                DiscountValue = "",
                EnabledForAccount = false,
                EnabledForSymbol = false,
            };

            public static readonly Order orderValid = new()
            {
                OrderId = 1,
                Side = "MARKET",
                Symbol = "ETH",
            };

            public static readonly Order orderUnvalid = new()
            {
                OrderId = null,
                Side = "MARKET",
                Symbol = null,
            };

            public static readonly Currency currencyValid = new()
            {
                Price = 1,
                Symbol = "ETHUSDT"
            };

            public static readonly Currency currencyUnvalid = new()
            {
                Price = null,
                Symbol = null
            };
        }
    }
}
