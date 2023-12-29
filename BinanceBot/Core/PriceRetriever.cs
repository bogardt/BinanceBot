using BinanceBot.Abstraction;
using BinanceBot.Strategy;
using Newtonsoft.Json;
using System.Globalization;

namespace BinanceBot.Core
{
    public class PriceRetriever : IPriceRetriever
    {
        private readonly IBinanceClient _binanceClient;
        private readonly ILogger _logger;

        public PriceRetriever(IBinanceClient binanceClient,
            ILogger logger)
        {
            _binanceClient = binanceClient;
            _logger = logger;
        }

        public List<decimal> GetClosingPrices(List<List<object>> klines)
        {
            var closingPrices = klines.Select((it) =>
            {
                if (!decimal.TryParse(it[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ret))
                    throw new InvalidCastException($"it cannot be converted to decimal for klines {JsonConvert.SerializeObject(it)}");
                return ret;
            }).ToList();

            return closingPrices;
        }

        public decimal CalculateMinimumSellingPrice(decimal cryptoPurchasePrice, decimal quantity, decimal feePercentage, decimal discount, decimal targetProfit)
        {
            decimal purchaseCommission = cryptoPurchasePrice * quantity * feePercentage;

            decimal discountOnPurchase = purchaseCommission * discount;
            decimal purchaseCommissionWithDiscount = purchaseCommission - discountOnPurchase;

            decimal effectiveCommissionRate = feePercentage * (1 - discount);

            decimal minimumSellingPrice = (cryptoPurchasePrice * quantity + purchaseCommissionWithDiscount + targetProfit) / (1 - effectiveCommissionRate);
            decimal minimumSellingPricePerItem = minimumSellingPrice / quantity;

            return minimumSellingPricePerItem;
        }

        public async Task HandleDiscount(TradingStrategy tradingStrategy)
        {
            var account = await _binanceClient.GetAcountInfosAsync();
            var bnbPrice = await _binanceClient.GetPriceBySymbolAsync("BNBUSDT");
            var re = await _binanceClient.GetCommissionBySymbolAsync(tradingStrategy.Symbol);

            var bnb = account.Balances.First(b => b.Asset == "BNB");
            var bnbFree = decimal.Parse(bnb.Free);
            var bnbIsBankable = bnbFree > 0;
            var usdtBnb = bnbFree * bnbPrice.Price;

            if (bnbIsBankable)
            {
                tradingStrategy.Discount = 1 - decimal.Parse(re.Discount.DiscountValue);
            }

            _logger.WriteLog($"SOL: {account.Balances.First(b => b.Asset == "SOL").Free} | " +
                 $"BNB: {account.Balances.First(b => b.Asset == "BNB").Free} | " +
                 $"BNBUSDT: {usdtBnb} | " +
                 $"USDT: {account.Balances.First(b => b.Asset == "USDT").Free} | " +
                 $"fee: {re.StandardCommission.Maker}" +
                 $"{(re.Discount.EnabledForSymbol && re.Discount.EnabledForAccount ?
                 $" | BNB discount: {re.Discount.DiscountValue}" : string.Empty)}");

        }
    }
}