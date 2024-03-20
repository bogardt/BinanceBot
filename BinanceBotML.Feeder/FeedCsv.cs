using BinanceBot.Abstraction;
using BinanceBot.BinanceApi.Model;
using BinanceBot.BinanceApi.Serializer;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Numerics;
using TradingCalculation;

namespace BinanceBotML.Feeder
{
    public class FeedCsv : IFeeder
    {
        private readonly IExchangeHttpClient _binanceClient;
        private readonly ITechnicalIndicatorsCalculator _technicalIndicatorsCalculator;
        private readonly JsonSerializerSettings _settings;

        public FeedCsv(IExchangeHttpClient binanceClient, ITechnicalIndicatorsCalculator technicalIndicatorsCalculator)
        {
            _binanceClient = binanceClient;
            _technicalIndicatorsCalculator = technicalIndicatorsCalculator;
            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new BinanceKlineConverter());
        }

        public async Task<List<BinanceKline>> GetFromDays(DateTime from, DateTime to)
        {
            var json = await _binanceClient.GetKLinesBySymbolAsyncStr(from, to, "SOLUSDT", "1m", "60");
            var klines = JsonConvert.DeserializeObject<List<BinanceKline>>(json, _settings);
            return klines;
        }
        public async Task<List<BinanceKline>> GetFromDaysOnSec(DateTime from, DateTime to, int batchSize)
        {
            var json = await _binanceClient.GetKLinesBySymbolAsyncStr(from, to, "SOLUSDT", "1s", batchSize.ToString());
            var klines = JsonConvert.DeserializeObject<List<BinanceKline>>(json, _settings);
            return klines;
        }

        public async Task Run10Min(string filePath)
        {
            var hours = 3;
            var from = DateTime.Now.AddHours(-3);
            var to = DateTime.Now.AddHours(-3).AddMinutes(10);
            var klines = new List<BinanceKline>();

            var j = 1;
            for (var i = -10; i < ((60 * hours) - 20); i += 10)
            {
                var kline = await GetFromDaysOnSec(from, to, 600);
                klines.AddRange(kline);
                from = from.AddMinutes(10);
                to = to.AddMinutes(10);
                Console.WriteLine($"add klines {j++} from {from} to {to}");
            }

            var str = "OpenDate,CloseDate,Open,High,Low,Close,Volume\r\n";
            foreach (var it in klines)
            //for (var i = 0; i < klines.Count; i++)
            {
                //var k = klines.Skip(i).Take(600);
                //var it = klines[i];
                //var sma = _technicalIndicatorsCalculator.CalculateMovingAverage(k.Select(x => x.Close), 600);

                str += $"{it.OpenTime},{it.CloseTime},{it.Open},{it.High},{it.Low},{it.Close},{it.Volume}\r\n";
            }

            File.WriteAllText(filePath, str);
        }
        public async Task Run(string filePath)
        {
            var days = 3;
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
