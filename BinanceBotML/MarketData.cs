using Microsoft.ML.Data;

namespace BinanceBotML
{
    public class MarketData
    {
        [LoadColumn(0)]
        public long OpenDate { get; set; }

        [LoadColumn(1)]
        public long CloseDate { get; set; }

        [LoadColumn(2)]
        public float Open { get; set; }

        [LoadColumn(3)]
        public float High { get; set; }

        [LoadColumn(4)]
        public float Low { get; set; }

        [LoadColumn(5)]
        public float Close { get; set; }

        [LoadColumn(6)]
        public float Volume { get; set; }
    }
}
