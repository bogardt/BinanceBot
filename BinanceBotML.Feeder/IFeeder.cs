
namespace BinanceBotML.Feeder
{
    public interface IFeeder
    {
        Task Run(string filePath);
        Task Run10Min(string filePath);

    }
}