using BinanceBot.BinanceApi.Model;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Serializer
{
    public class BinanceKlineConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(BinanceKline));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JArray array = JArray.Load(reader);
            return new BinanceKline
            {
                OpenTime = (long)array[0],
                Open = (decimal)array[1],
                High = (decimal)array[2],
                Low = (decimal)array[3],
                Close = (decimal)array[4],
                Volume = (decimal)array[5],
                CloseTime = (long)array[6],
                QuoteAssetVolume = (decimal)array[7],
                NumberOfTrades = (int)array[8],
                TakerBuyBaseAssetVolume = (decimal)array[9],
                TakerBuyQuoteAssetVolume = (decimal)array[10],
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
