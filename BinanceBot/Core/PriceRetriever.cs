using BinanceBot.Abstraction;
using Newtonsoft.Json;
using System.Globalization;

namespace BinanceBot.Core
{
    public class PriceRetriever : IPriceRetriever
    {
        public List<decimal> GetRecentPrices(List<List<object>> klines)
        {
            var closingPrices = new List<decimal>();
            foreach (var kline in klines)
            {
                if (!decimal.TryParse(kline[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ret))
                    throw new InvalidCastException($"it cannot be converted to decimal for klines {JsonConvert.SerializeObject(kline)}");
                closingPrices.Add(ret); // 4 is the index for closing price
            }
            return closingPrices;
        }
    }
}