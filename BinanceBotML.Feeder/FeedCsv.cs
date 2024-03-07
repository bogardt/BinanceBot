using BinanceBot.Abstraction;
using BinanceBot.BinanceApi.Model;
using BinanceBot.BinanceApi.Serializer;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace BinanceBotML.Feeder
{
    public class FeedCsv : IFeeder
    {
        private readonly IExchangeHttpClient _binanceClient;
        private readonly JsonSerializerSettings _settings;

        public FeedCsv(IExchangeHttpClient binanceClient)
        {
            _binanceClient = binanceClient;

            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new BinanceKlineConverter());
        }

        public async Task<List<BinanceKline>> GetFromDays(DateTime from, DateTime to)
        {
            var json = await _binanceClient.GetKLinesBySymbolAsyncStr(from, to, "SOLUSDT", "1m", "60");
            var klines = JsonConvert.DeserializeObject<List<BinanceKline>>(json, _settings);
            return klines;
        }

        public async Task Run(string filePath)
        {
            var days = 7;
            var from = DateTime.Now.AddDays(-days);
            var to = DateTime.Now.AddDays(-days).AddHours(1);
            var klines = new List<BinanceKline>();

            for (var i = 0; i < (24 * days); i++)
            {
                var kline = await GetFromDays(from, to);
                klines.AddRange(kline);
                from = from.AddHours(1);
                to = to.AddHours(1);
                Console.WriteLine($"add klines {i} from {from} to {to}");
            }


            var str = "OpenDate,CloseDate,Open,High,Low,Close,Volume\r\n";
            foreach (var it in klines)
            {
                str += $"{it.OpenTime},{it.CloseTime},{it.Open},{it.High},{it.Low},{it.Close},{it.Volume}\r\n";
            }

            File.WriteAllText(filePath, str);
        }

    }
}
