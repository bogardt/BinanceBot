using BinanceBot.Abstraction;
using BinanceBot.Strategy;
using Newtonsoft.Json;
using System.Globalization;

namespace BinanceBot.Core;

public class PriceRetriever(
    IBinanceClient binanceClient,
    ILogger logger) : IPriceRetriever
{
    private readonly IBinanceClient _binanceClient = binanceClient;
    private readonly ILogger _logger = logger;

    public List<decimal> GetClosingPrices(List<List<object>> klines) => klines.Select((it) =>
    {
        if (!decimal.TryParse(it[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ret))
            throw new InvalidCastException($"it cannot be converted to decimal for klines {JsonConvert.SerializeObject(it)}");
        return ret;
    }).ToList();

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

    public async Task HandleDiscountAsync(TradingStrategy tradingStrategy)
    {
        var commission = await _binanceClient.GetCommissionBySymbolAsync(tradingStrategy.Symbol);
        var account = await _binanceClient.GetAccountInfosAsync();
        var bnbPrice = await _binanceClient.GetPriceBySymbolAsync("BNBUSDT");
        var bnb = account.Balances.First(b => b.Asset == "BNB");
        var bnbFree = decimal.Parse(bnb.Free);
        var usdtBnb = bnbFree * bnbPrice.Price;
        var bnbIsBankable = usdtBnb > 100;

        if (bnbIsBankable)
        {
            tradingStrategy.Discount = 1 - decimal.Parse(commission.Discount.DiscountValue);
        }

        _logger.WriteLog($"SOL: {account.Balances.First(b => b.Asset == "SOL").Free} | " +
             $"BNB: {account.Balances.First(b => b.Asset == "BNB").Free} | " +
             $"BNBUSDT: {usdtBnb} | " +
             $"USDT: {account.Balances.First(b => b.Asset == "USDT").Free} | " +
             $"fee: {commission.StandardCommission.Maker}" +
             $"{(commission.Discount.EnabledForSymbol && commission.Discount.EnabledForAccount ?
             $" | BNB discount: {commission.Discount.DiscountValue}" : string.Empty)}");
    }
}