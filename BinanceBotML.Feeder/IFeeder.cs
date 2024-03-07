
namespace BinanceBotML.Feeder
{
    public interface IFeeder
    {
        Task Run(string filePath);
    }
}