namespace BinanceBot.Abstraction
{
    public interface ITechnicalIndicatorsCalculator
    {
        decimal CalculateMovingAverage(List<List<object>> klines, int periode);
        decimal CalculateRSI(List<List<object>> klines, int periode);
    }
}