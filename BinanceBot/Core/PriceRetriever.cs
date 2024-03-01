using BinanceBot.Abstraction;
using Newtonsoft.Json;
using System.Globalization;
using TradingCalculation.Strategy;

namespace BinanceBot.Core;

public class PriceRetriever(
    IExchangeHttpClient binanceClient,
    ILogger logger) : IPriceRetriever
{
    //public IEnumerable<decimal> GetClosingPrices(IEnumerable<IEnumerable<object>> klines) => klines.Select((it) =>
    //{
    //    var closingPrice = it.ElementAt(4).ToString();
    //    if (!decimal.TryParse(closingPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var ret))
    //        throw new InvalidCastException($"it cannot be converted to decimal for klines {JsonConvert.SerializeObject(it)}");
    //    return ret;
    //});
    public List<decimal> GetClosingPrices(List<List<object>> klines)
    {
        var ret = new List<decimal>();
        foreach (var it in klines)
        {
            var kline = it.ToList();
            var closingPrice = kline[4].ToString();
            if (!decimal.TryParse(closingPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                throw new InvalidCastException($"it cannot be converted to decimal for klines {JsonConvert.SerializeObject(kline)}");
            ret.Add(dec);
        }
        return ret;
    }

    public async Task HandleDiscountAsync(TradingStrategy tradingStrategy)
    {
        var commission = await binanceClient.GetCommissionBySymbolAsync(tradingStrategy.Symbol);
        var account = await binanceClient.GetAccountInfosAsync();
        var bnbPrice = await binanceClient.GetPriceBySymbolAsync("BNBUSDT");
        var bnb = account.Balances?.First(b => b.Asset == "BNB");
        var bnbFree = decimal.Parse(bnb?.Free!);
        var usdtBnb = bnbFree * bnbPrice.Price;
        var bnbIsBankable = usdtBnb > 100;

        if (bnbIsBankable)
        {
            tradingStrategy.Discount = 1 - decimal.Parse(commission?.Discount?.DiscountValue!);
            logger.WriteLog($"SOL: {account?.Balances?.First(b => b.Asset == "SOL").Free} | " +
                 $"BNB: {account?.Balances?.First(b => b.Asset == "BNB").Free} | " +
                 $"BNBUSDT: {usdtBnb} | " +
                 $"USDT: {account?.Balances?.First(b => b.Asset == "USDT").Free} | " +
                 $"fee: {commission?.StandardCommission?.Maker}" +
                 $"{(commission?.Discount?.EnabledForSymbol == true && commission.Discount.EnabledForAccount == true ?
                 $" | BNB discount: {commission.Discount.DiscountValue}" : string.Empty)}");
        }


    }
}