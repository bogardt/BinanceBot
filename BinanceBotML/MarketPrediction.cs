using Microsoft.ML.Data;

namespace BinanceBotML
{
    public class MarketPrediction
    {
        [ColumnName("Score")]
        public float Close { get; set; }
    }
}
