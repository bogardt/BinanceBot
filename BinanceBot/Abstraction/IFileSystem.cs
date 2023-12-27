namespace BinanceBot.Abstraction
{
    public interface IFileSystem
    {
        string GetCurrentDirectory();
        FileInfo[] GetFiles(string path, string searchPattern);
    }
}
